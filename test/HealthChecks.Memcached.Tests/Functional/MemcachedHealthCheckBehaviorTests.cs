using Enyim.Caching;
using NSubstitute;

namespace HealthChecks.Memcached.Tests.Functional;

public class memcached_healthcheck_behavior_should
{
    [Fact]
    public async Task be_unhealthy_if_cache_item_cannot_be_stored()
    {
        IMemcachedClient memcachedClient = Substitute.For<IMemcachedClient>();
        memcachedClient
            .SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>())
            .Returns(Task.FromResult(false));

        var healthCheck = new MemcachedHealthCheck(memcachedClient);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("memcached", healthCheck, HealthStatus.Degraded, tags: null)
        };

        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldBe("Memcached cache item could not be stored.");
    }

    [Fact]
    public async Task be_unhealthy_if_cache_item_cannot_be_read_back()
    {
        IMemcachedClient memcachedClient = Substitute.For<IMemcachedClient>();
        memcachedClient
            .SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>())
            .Returns(Task.FromResult(true));
        memcachedClient
            .GetValueAsync<string>(Arg.Any<string>())
            .Returns(Task.FromResult("unexpected"));
        memcachedClient
            .RemoveAsync(Arg.Any<string>())
            .Returns(Task.FromResult(true));

        var healthCheck = new MemcachedHealthCheck(memcachedClient);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("memcached", healthCheck, HealthStatus.Unhealthy, tags: null)
        };

        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldBe("Memcached cache item could not be read back.");
    }
}
