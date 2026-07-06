using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Grafana;

/// <summary>
/// A health check for Grafana services.
/// </summary>
public sealed class GrafanaHealthCheck : IHealthCheck
{
    private const string OK_STATUS = "ok";

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly GrafanaHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the Grafana health check.
    /// </summary>
    /// <param name="client">The HTTP client used to call the Grafana API.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public GrafanaHealthCheck(HttpClient client, GrafanaHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new GrafanaHealthCheckOptions();
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
                    description: $"Grafana health endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            GrafanaHealthResponse? health = JsonSerializer.Deserialize<GrafanaHealthResponse>(responseContent, _serializerOptions);

            return CreateResult(context, health);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private HealthCheckResult CreateResult(HealthCheckContext context, GrafanaHealthResponse? health)
    {
        if (health is null || string.IsNullOrWhiteSpace(health.Database))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "Grafana health response did not include a database status value.");
        }

        Dictionary<string, object> data = CreateData(health);
        string database = health.Database!;

        if (!IsOkStatus(database))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Grafana reported database status '{database}'.",
                data: data);
        }

        return HealthCheckResult.Healthy(data: data);
    }

    private Uri GetHealthEndpointUri()
    {
        if (string.IsNullOrWhiteSpace(_options.HealthEndpointPath))
        {
            throw new InvalidOperationException($"{nameof(GrafanaHealthCheckOptions.HealthEndpointPath)} must be configured.");
        }

        if (Uri.TryCreate(_options.HealthEndpointPath, UriKind.Absolute, out Uri? absoluteUri) && !absoluteUri.IsFile)
        {
            return absoluteUri;
        }

        Uri? baseUri = (_options.BaseUri ?? _client.BaseAddress)
            ?? throw new InvalidOperationException($"{nameof(GrafanaHealthCheckOptions.BaseUri)} or {nameof(HttpClient.BaseAddress)} must be configured.");

        return new Uri(baseUri, _options.HealthEndpointPath);
    }

    private static Dictionary<string, object> CreateData(GrafanaHealthResponse health)
    {
        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "database", health.Database! }
        };

        if (!string.IsNullOrWhiteSpace(health.Version))
        {
            data["version"] = health.Version!;
        }

        if (!string.IsNullOrWhiteSpace(health.Commit))
        {
            data["commit"] = health.Commit!;
        }

        return data;
    }

    private static bool IsOkStatus(string? status)
        => string.Equals(status, OK_STATUS, StringComparison.OrdinalIgnoreCase);

    private sealed class GrafanaHealthResponse
    {
        [JsonPropertyName("commit")]
        public string? Commit { get; set; }

        [JsonPropertyName("database")]
        public string? Database { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }
}
