namespace HealthChecks.Apache.Pulsar;

/// <summary>
/// Represents settings used by <see cref="PulsarHealthCheck"/>.
/// </summary>
public sealed class PulsarHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the Pulsar topic used to verify broker connectivity.
    /// </summary>
    public string Topic { get; set; } = "persistent://public/default/healthchecks";

    /// <summary>
    /// Gets or sets the delegate that creates the health check message payload.
    /// </summary>
    public Func<PulsarHealthCheckOptions, ReadOnlyMemory<byte>> MessageBuilder { get; set; } = static _ => Array.Empty<byte>();
}
