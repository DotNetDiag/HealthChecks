using Apache.NMS;

namespace HealthChecks.ActiveMQ;

/// <summary>
/// Options for <see cref="ActiveMQHealthCheck"/>.
/// </summary>
public sealed class ActiveMQHealthCheckOptions
{
    /// <summary>
    /// The optional user name used when creating an ActiveMQ connection.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The optional password used when creating an ActiveMQ connection.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The acknowledgement mode used when creating an ActiveMQ session.
    /// </summary>
    public AcknowledgementMode AcknowledgementMode { get; set; } = AcknowledgementMode.AutoAcknowledge;

    /// <summary>
    /// The optional request timeout assigned to the created ActiveMQ connection.
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }
}
