namespace HealthChecks.Apache.Pulsar;

/// <summary>
/// Represents settings used by <see cref="PulsarAdminHealthCheck"/>.
/// </summary>
public sealed class PulsarAdminHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the Apache Pulsar admin health endpoint.
    /// </summary>
    public string HealthEndpoint { get; set; } = "/admin/v2/brokers/health";

    /// <summary>
    /// Gets or sets the delegate used to validate the admin health response.
    /// </summary>
    public Func<HttpResponseMessage, bool> ResponseValidator { get; set; } = static response => response.IsSuccessStatusCode;
}
