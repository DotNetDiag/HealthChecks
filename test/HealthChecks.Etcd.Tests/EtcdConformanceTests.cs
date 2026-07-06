namespace HealthChecks.Etcd.Tests;

public class EtcdConformanceTests : ConformanceTests<string, EtcdHealthCheck, EtcdHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, string>? clientFactory = default,
        Func<IServiceProvider, EtcdHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddEtcd(clientFactory, optionsFactory ?? (_ => CreateHealthCheckOptions()), healthCheckName, failureStatus, tags, timeout);
    }

    protected override string CreateClientForNonExistingEndpoint()
        => "http://127.0.0.1:1";

    protected override EtcdHealthCheck CreateHealthCheck(string client, EtcdHealthCheckOptions? options)
        => new(client, options ?? CreateHealthCheckOptions());

    protected override EtcdHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
