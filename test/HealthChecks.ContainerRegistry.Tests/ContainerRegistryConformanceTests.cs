namespace HealthChecks.ContainerRegistry.Tests;

public class ContainerRegistryConformanceTests : ConformanceTests<HttpClient, ContainerRegistryHealthCheck, ContainerRegistryHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, ContainerRegistryHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddContainerRegistry(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override ContainerRegistryHealthCheck CreateHealthCheck(HttpClient client, ContainerRegistryHealthCheckOptions? options)
        => new(client, options);

    protected override ContainerRegistryHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
