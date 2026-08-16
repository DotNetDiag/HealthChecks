namespace HealthChecks.IoTDB;

/// <summary>
/// Options for <see cref="IoTDBHealthCheck"/>.
/// </summary>
public class IoTDBHealthCheckOptions
{
    /// <summary>
    /// The IoTDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable RPC compression.
    /// </summary>
    public bool EnableRpcCompression { get; set; }
}
