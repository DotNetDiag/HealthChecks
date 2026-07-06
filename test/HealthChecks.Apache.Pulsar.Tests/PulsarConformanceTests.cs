using DotPulsar.Abstractions;

namespace HealthChecks.Apache.Pulsar.Tests;

public class PulsarConformanceTests : ConformanceTests<IPulsarClient, PulsarHealthCheck, PulsarHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, IPulsarClient>? clientFactory = default,
        Func<IServiceProvider, PulsarHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddPulsar(clientFactory, optionsFactory ?? (_ => CreateHealthCheckOptions()), healthCheckName, failureStatus, tags, timeout);
    }

    protected override IPulsarClient CreateClientForNonExistingEndpoint()
        => PulsarTestClientFactory.CreateUnavailableClient();

    protected override PulsarHealthCheck CreateHealthCheck(IPulsarClient client, PulsarHealthCheckOptions? options)
        => new(client, options ?? CreateHealthCheckOptions());

    protected override PulsarHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
