namespace HealthChecks.ZooKeeper.Tests;

public class ZooKeeperConformanceTests : ConformanceTests<string, ZooKeeperHealthCheck, ZooKeeperHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, string>? clientFactory = default,
        Func<IServiceProvider, ZooKeeperHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddZooKeeper(clientFactory, optionsFactory ?? (_ => CreateHealthCheckOptions()), healthCheckName, failureStatus, tags, timeout);
    }

    protected override string CreateClientForNonExistingEndpoint()
        => "127.0.0.1:1";

    protected override ZooKeeperHealthCheck CreateHealthCheck(string client, ZooKeeperHealthCheckOptions? options)
        => new(client, options ?? CreateHealthCheckOptions());

    protected override ZooKeeperHealthCheckOptions CreateHealthCheckOptions()
        => new()
        {
            SessionTimeout = TimeSpan.FromMilliseconds(500)
        };
}
