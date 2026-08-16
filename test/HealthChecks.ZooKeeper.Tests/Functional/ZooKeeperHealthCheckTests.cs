using System.Net;

namespace HealthChecks.ZooKeeper.Tests.Functional;

public class zookeeper_healthcheck_should(ZooKeeperContainerFixture zooKeeperContainerFixture) : IClassFixture<ZooKeeperContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_zookeeper_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddZooKeeper(zooKeeperContainerFixture.GetConnectionString(), tags: ["zookeeper"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("zookeeper")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_zookeeper_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddZooKeeper(
                        "127.0.0.1:1",
                        options => options.SessionTimeout = TimeSpan.FromMilliseconds(500),
                        tags: ["zookeeper"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("zookeeper")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_znode_does_not_exist()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddZooKeeper(
                        zooKeeperContainerFixture.GetConnectionString(),
                        options => options.Path = "/healthchecks-znode-does-not-exist",
                        tags: ["zookeeper"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("zookeeper")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
