using System.Net;

namespace HealthChecks.Apache.Pulsar.Tests.Functional;

public class pulsar_admin_healthcheck_should(PulsarContainerFixture pulsarContainerFixture) : IClassFixture<PulsarContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_pulsar_admin_endpoint_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddPulsarAdmin(new Uri(pulsarContainerFixture.GetHttpServiceUrl()), tags: ["pulsar_admin"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar_admin")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public class pulsar_admin_unavailable_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_pulsar_admin_endpoint_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddPulsarAdmin(new Uri("http://127.0.0.1:1"), tags: ["pulsar_admin"], timeout: TimeSpan.FromSeconds(5));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar_admin")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
