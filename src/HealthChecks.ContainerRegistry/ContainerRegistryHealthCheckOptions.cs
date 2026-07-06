namespace HealthChecks.ContainerRegistry;

/// <summary>
/// Represents settings used by <see cref="ContainerRegistryHealthCheck"/>.
/// </summary>
public sealed class ContainerRegistryHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the container registry base URI. When not configured, <see cref="HttpClient.BaseAddress"/> is used.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the OCI/Docker Registry HTTP API v2 endpoint path.
    /// </summary>
    public string RegistryEndpointPath { get; set; } = "/v2/";

    /// <summary>
    /// Gets or sets a value indicating whether HTTP 401 responses can be reported as healthy when the registry requires authentication.
    /// </summary>
    public bool AllowUnauthorizedResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether HTTP 401 responses must include a WWW-Authenticate challenge to be reported as healthy.
    /// </summary>
    public bool RequireAuthenticationChallenge { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional callback that can customize the outgoing request.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }
}
