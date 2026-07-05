using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Harbor;

/// <summary>
/// A health check for Harbor services.
/// </summary>
public sealed class HarborHealthCheck : IHealthCheck
{
    private const string HEALTHY_STATUS = "healthy";

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly HarborHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the Harbor health check.
    /// </summary>
    /// <param name="client">The HTTP client used to call the Harbor API.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public HarborHealthCheck(HttpClient client, HarborHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new HarborHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetHealthEndpointUri());
            _options.ConfigureRequest?.Invoke(request);

            using HttpResponseMessage response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"Harbor health endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            HarborHealthResponse? health = JsonSerializer.Deserialize<HarborHealthResponse>(responseContent, _serializerOptions);

            return CreateResult(context, health);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private HealthCheckResult CreateResult(HealthCheckContext context, HarborHealthResponse? health)
    {
        if (health is null || string.IsNullOrWhiteSpace(health.Status))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "Harbor health response did not include a status value.");
        }

        string status = health.Status!;
        Dictionary<string, object> data = CreateData(health);
        string[] unhealthyComponents = GetUnhealthyComponents(health.Components);

        if (!IsHealthyStatus(status))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: CreateUnhealthyDescription(status, unhealthyComponents),
                data: data);
        }

        if (unhealthyComponents.Length != 0)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Harbor component status is not healthy: {string.Join(", ", unhealthyComponents)}.",
                data: data);
        }

        string[] missingComponents = GetMissingComponents(health.Components);

        if (missingComponents.Length != 0)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Harbor health response did not include required components: {string.Join(", ", missingComponents)}.",
                data: data);
        }

        return HealthCheckResult.Healthy(data: data);
    }

    private Uri GetHealthEndpointUri()
    {
        if (string.IsNullOrWhiteSpace(_options.HealthEndpointPath))
        {
            throw new InvalidOperationException($"{nameof(HarborHealthCheckOptions.HealthEndpointPath)} must be configured.");
        }

        if (Uri.TryCreate(_options.HealthEndpointPath, UriKind.Absolute, out Uri? absoluteUri))
        {
            return absoluteUri;
        }

        Uri? baseUri = (_options.BaseUri ?? _client.BaseAddress) ?? throw new InvalidOperationException($"{nameof(HarborHealthCheckOptions.BaseUri)} or {nameof(HttpClient.BaseAddress)} must be configured.");

        return new Uri(baseUri, _options.HealthEndpointPath);
    }

    private string[] GetMissingComponents(IEnumerable<HarborComponentHealth>? components)
    {
        if (_options.RequiredComponents.Count == 0)
        {
            return [];
        }

        IEnumerable<string> names = components is null
            ? []
            : components
                .Select(component => component.Name!)
                .Where(name => !string.IsNullOrWhiteSpace(name));

        var componentNames = new HashSet<string>(
            names,
            StringComparer.OrdinalIgnoreCase);

        return _options.RequiredComponents
            .Where(component => !componentNames.Contains(component))
            .OrderBy(component => component, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static Dictionary<string, object> CreateData(HarborHealthResponse health)
    {
        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "status", health.Status! }
        };

        if (health.Components is null)
        {
            return data;
        }

        foreach (HarborComponentHealth component in health.Components)
        {
            if (!string.IsNullOrWhiteSpace(component.Name))
            {
                data[$"component:{component.Name}"] = component.Status ?? string.Empty;
            }
        }

        return data;
    }

    private static string[] GetUnhealthyComponents(IEnumerable<HarborComponentHealth>? components)
    {
        return components?
            .Where(component => !string.IsNullOrWhiteSpace(component.Name) && !IsHealthyStatus(component.Status))
            .Select(component => $"{component.Name}={component.Status ?? "<missing>"}")
            .OrderBy(component => component, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }

    private static string CreateUnhealthyDescription(string status, string[] unhealthyComponents)
    {
        if (unhealthyComponents.Length == 0)
        {
            return $"Harbor reported status '{status}'.";
        }

        return $"Harbor reported status '{status}'. Unhealthy components: {string.Join(", ", unhealthyComponents)}.";
    }

    private static bool IsHealthyStatus(string? status)
        => string.Equals(status, HEALTHY_STATUS, StringComparison.OrdinalIgnoreCase);

    private sealed class HarborHealthResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("components")]
        public List<HarborComponentHealth>? Components { get; set; }
    }

    private sealed class HarborComponentHealth
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
