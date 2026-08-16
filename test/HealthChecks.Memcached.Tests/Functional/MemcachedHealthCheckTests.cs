using System.Net;
using Enyim.Caching;

namespace HealthChecks.Memcached.Tests.Functional;

public class memcached_healthcheck_should(MemcachedContainerFixture memcachedContainerFixture) : IClassFixture<MemcachedContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_memcached_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddMemcached(memcachedContainerFixture.Host, memcachedContainerFixture.Port, tags: ["memcached"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("memcached")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_memcached_is_available_with_options()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddMemcached(options =>
                    {
                        options.AddServer(memcachedContainerFixture.Host, memcachedContainerFixture.Port);
                        options.CacheItemExpiration = TimeSpan.FromSeconds(5);
                        options.KeyPrefix = "healthchecks_memcached_options_";
                    }, tags: ["memcached"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("memcached")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_memcached_is_available_with_client()
    {
        IMemcachedClient memcachedClient = MemcachedClientFactory.Create(memcachedContainerFixture.Host, memcachedContainerFixture.Port);

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddMemcached(memcachedClient, tags: ["memcached"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("memcached")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_memcached_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddMemcached(options =>
                    {
                        options.AddServer("127.0.0.1", 1);
                        options.ClientOptions.SocketPool.ConnectionTimeout = TimeSpan.FromMilliseconds(200);
                        options.ClientOptions.SocketPool.ReceiveTimeout = TimeSpan.FromMilliseconds(200);
                        options.ClientOptions.SocketPool.QueueTimeout = TimeSpan.FromMilliseconds(200);
                    }, tags: ["memcached"], timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("memcached")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
