using org.apache.zookeeper;

namespace HealthChecks.ZooKeeper;

internal sealed class NoOpWatcher : Watcher
{
    public static readonly NoOpWatcher Instance = new();

    private NoOpWatcher()
    {
    }

    public override Task process(WatchedEvent @event) => Task.CompletedTask;
}
