using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class ping_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_all_hosts_reply_to_ping()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddPingHealthCheck(setup => setup.AddHost("127.0.0.1", 5000), tags: ["ping"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ping")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_a_host_ping_is_not_successful()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddPingHealthCheck(setup =>
                {
                    setup.AddHost("127.0.0.1", 3000);
                    setup.AddHost("nonexistinghost", 3000);
                }, tags: ["ping"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ping")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
