using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SonnetDB;

/// <summary>
/// A health check for SonnetDB services.
/// </summary>
public sealed class SonnetDBHealthCheck : IHealthCheck
{
    private const string HEALTHY_STATUS = "ok";

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly SonnetDBHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the SonnetDB health check.
    /// </summary>
    /// <param name="client">The HTTP client used to call the SonnetDB API.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public SonnetDBHealthCheck(HttpClient client, SonnetDBHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new SonnetDBHealthCheckOptions();
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
                    description: $"SonnetDB health endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            SonnetDBHealthResponse? health = JsonSerializer.Deserialize<SonnetDBHealthResponse>(responseContent, _serializerOptions);

            return CreateResult(context, health);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private HealthCheckResult CreateResult(HealthCheckContext context, SonnetDBHealthResponse? health)
    {
        if (health is null || string.IsNullOrWhiteSpace(health.Status))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "SonnetDB health response did not include a status value.");
        }

        Dictionary<string, object> data = CreateData(health);
        string status = health.Status!;

        if (!string.Equals(status, HEALTHY_STATUS, StringComparison.OrdinalIgnoreCase))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"SonnetDB reported status '{status}'.",
                data: data);
        }

        if (_options.RequireCopilotReady && health.CopilotEnabled == true && health.CopilotReady != true)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "SonnetDB Copilot is enabled but not ready.",
                data: data);
        }

        return HealthCheckResult.Healthy(data: data);
    }

    private Uri GetHealthEndpointUri()
    {
        if (string.IsNullOrWhiteSpace(_options.HealthEndpointPath))
        {
            throw new InvalidOperationException($"{nameof(SonnetDBHealthCheckOptions.HealthEndpointPath)} must be configured.");
        }

        if (Uri.TryCreate(_options.HealthEndpointPath, UriKind.Absolute, out Uri? absoluteUri) && !absoluteUri.IsFile)
        {
            return absoluteUri;
        }

        Uri? baseUri = (_options.BaseUri ?? _client.BaseAddress) ?? throw new InvalidOperationException($"{nameof(SonnetDBHealthCheckOptions.BaseUri)} or {nameof(HttpClient.BaseAddress)} must be configured.");

        return new Uri(baseUri, _options.HealthEndpointPath);
    }

    private static Dictionary<string, object> CreateData(SonnetDBHealthResponse health)
    {
        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "status", health.Status! }
        };

        if (health.Databases.HasValue)
        {
            data["databases"] = health.Databases.Value;
        }

        if (health.UptimeSeconds.HasValue)
        {
            data["uptimeSeconds"] = health.UptimeSeconds.Value;
        }

        if (health.CopilotEnabled.HasValue)
        {
            data["copilotEnabled"] = health.CopilotEnabled.Value;
        }

        if (health.CopilotReady.HasValue)
        {
            data["copilotReady"] = health.CopilotReady.Value;
        }

        return data;
    }

    private sealed class SonnetDBHealthResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("databases")]
        public int? Databases { get; set; }

        [JsonPropertyName("uptimeSeconds")]
        public double? UptimeSeconds { get; set; }

        [JsonPropertyName("copilotEnabled")]
        public bool? CopilotEnabled { get; set; }

        [JsonPropertyName("copilotReady")]
        public bool? CopilotReady { get; set; }
    }
}
