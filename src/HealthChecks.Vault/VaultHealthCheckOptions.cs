namespace HealthChecks.Vault;

/// <summary>
/// Represents settings used by <see cref="VaultHealthCheck"/>.
/// </summary>
public sealed class VaultHealthCheckOptions
{
    private readonly HashSet<VaultHealthStatus> _healthyStatuses = [VaultHealthStatus.Active];

    /// <summary>
    /// Gets or sets the Vault base URI. When not configured, <see cref="HttpClient.BaseAddress"/> is used.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the Vault system health endpoint path.
    /// </summary>
    public string HealthEndpointPath { get; set; } = "/v1/sys/health";

    /// <summary>
    /// Gets the Vault statuses that should be reported as healthy.
    /// </summary>
    public ISet<VaultHealthStatus> HealthyStatuses => _healthyStatuses;

    /// <summary>
    /// Gets or sets an optional callback that can customize the outgoing request.
    /// </summary>
    public Action<HttpRequestMessage>? ConfigureRequest { get; set; }
}
