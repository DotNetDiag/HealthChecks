using System.Net;

namespace HealthChecks.Etcd.Tests.Functional;

public class etcd_healthcheck_should(EtcdContainerFixture etcdContainerFixture) : IClassFixture<EtcdContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_etcd_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEtcd(etcdContainerFixture.GetConnectionString(), tags: ["etcd"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("etcd")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_etcd_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddEtcd("http://127.0.0.1:1", tags: ["etcd"], timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("etcd")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
