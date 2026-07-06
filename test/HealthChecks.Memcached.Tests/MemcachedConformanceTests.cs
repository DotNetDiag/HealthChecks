using Enyim.Caching;

namespace HealthChecks.Memcached.Tests;

public class MemcachedConformanceTests : ConformanceTests<IMemcachedClient, MemcachedHealthCheck, MemcachedHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, IMemcachedClient>? clientFactory = default,
        Func<IServiceProvider, MemcachedHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddMemcached(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override IMemcachedClient CreateClientForNonExistingEndpoint()
    {
        return MemcachedClientFactory.Create("127.0.0.1", 1, TimeSpan.FromMilliseconds(200));
    }

    protected override MemcachedHealthCheck CreateHealthCheck(IMemcachedClient client, MemcachedHealthCheckOptions? options)
    {
        return new MemcachedHealthCheck(client, options);
    }

    protected override MemcachedHealthCheckOptions CreateHealthCheckOptions()
    {
        return new MemcachedHealthCheckOptions();
    }
}
