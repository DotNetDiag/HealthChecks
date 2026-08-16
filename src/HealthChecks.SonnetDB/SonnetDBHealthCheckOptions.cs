namespace HealthChecks.SonnetDB;

/// <summary>
/// Represents settings used by <see cref="SonnetDBHealthCheck"/>.
/// </summary>
public sealed class SonnetDBHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the SonnetDB base URI. When not configured, <see cref="HttpClient.BaseAddress"/> is used.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the SonnetDB health endpoint path.
    /// </summary>
    public string HealthEndpointPath { get; set; } = "/healthz";

    /// <summary>
    /// Gets or sets a value indicating whether the health check should fail when Copilot is enabled but not ready.
    /// </summary>
    public bool RequireCopilotReady { get; set; }

    /// <summary>
    /// Gets or sets an optional callback that can customize the outgoing request.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }
}
