namespace HealthChecks.Harbor.Tests;

public class HarborConformanceTests : ConformanceTests<HttpClient, HarborHealthCheck, HarborHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, HarborHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddHarbor(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override HarborHealthCheck CreateHealthCheck(HttpClient client, HarborHealthCheckOptions? options)
        => new(client, options);

    protected override HarborHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
