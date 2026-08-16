namespace HealthChecks.Harbor;

/// <summary>
/// Represents settings used by <see cref="HarborHealthCheck"/>.
/// </summary>
public sealed class HarborHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the Harbor base URI. When not configured, <see cref="HttpClient.BaseAddress"/> is used.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the Harbor health endpoint path.
    /// </summary>
    public string HealthEndpointPath { get; set; } = "/api/v2.0/health";

    /// <summary>
    /// Gets the Harbor components that must be present in the health response.
    /// </summary>
    public ISet<string> RequiredComponents { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets an optional callback that can customize the outgoing request.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }
}
