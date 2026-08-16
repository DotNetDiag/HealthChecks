namespace HealthChecks.Cassandra;

/// <summary>
/// Options for <see cref="CassandraHealthCheck"/>.
/// </summary>
public class CassandraHealthCheckOptions
{
    /// <summary>
    /// The keyspace to connect to. If null, connects without a specific keyspace.
    /// </summary>
    public string? Keyspace { get; set; }
}
