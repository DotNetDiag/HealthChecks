namespace HealthChecks.Apache.Pulsar.Tests;

public class PulsarAdminConformanceTests : ConformanceTests<HttpClient, PulsarAdminHealthCheck, PulsarAdminHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, PulsarAdminHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddPulsarAdmin(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override PulsarAdminHealthCheck CreateHealthCheck(HttpClient client, PulsarAdminHealthCheckOptions? options)
        => new(client, options);

    protected override PulsarAdminHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
