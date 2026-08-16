namespace HealthChecks.NTPServer;

/// <summary>
/// Options for <see cref="NTPServerHealthCheck"/>.
/// </summary>
public class NTPServerHealthCheckOptions
{
    /// <summary>
    /// The NTP server hostname. Defaults to "pool.ntp.org".
    /// </summary>
    public string NtpServer { get; set; } = "pool.ntp.org";

    /// <summary>
    /// The NTP server UDP port. Defaults to 123.
    /// </summary>
    public int NtpPort { get; set; } = 123;

    /// <summary>
    /// The maximum allowed time offset in seconds before returning Unhealthy. Defaults to 10.0.
    /// </summary>
    public double ToleranceSeconds { get; set; } = 10.0;
}
