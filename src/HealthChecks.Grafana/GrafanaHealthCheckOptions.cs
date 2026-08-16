namespace HealthChecks.Grafana;

/// <summary>
/// Represents settings used by <see cref="GrafanaHealthCheck"/>.
/// </summary>
public sealed class GrafanaHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the Grafana base URI. When not configured, <see cref="HttpClient.BaseAddress"/> is used.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the Grafana health endpoint path.
    /// </summary>
    public string HealthEndpointPath { get; set; } = "/api/health";

    /// <summary>
    /// Gets or sets an optional callback that can customize the outgoing request.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }
}
