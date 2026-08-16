using org.apache.zookeeper;

namespace HealthChecks.ZooKeeper;

/// <summary>
/// Represents settings used by <see cref="ZooKeeperHealthCheck"/>.
/// </summary>
public sealed class ZooKeeperHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the znode path checked by the health check.
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Gets or sets the ZooKeeper session timeout.
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether the ZooKeeper client can connect to read-only servers.
    /// </summary>
    public bool CanBeReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a factory that creates the watcher used by the ZooKeeper client.
    /// </summary>
    public Func<Watcher> WatcherFactory { get; set; } = static () => NoOpWatcher.Instance;
}
