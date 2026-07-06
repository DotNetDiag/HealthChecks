using Apache.NMS;

namespace HealthChecks.Artemis;

/// <summary>
/// Options for <see cref="ArtemisHealthCheck"/>.
/// </summary>
public sealed class ArtemisHealthCheckOptions
{
    /// <summary>
    /// The optional user name used when creating an Artemis connection.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The optional password used when creating an Artemis connection.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The acknowledgement mode used when creating an Artemis session.
    /// </summary>
    public AcknowledgementMode AcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

    /// <summary>
    /// The optional request timeout assigned to the created Artemis connection.
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }
}
