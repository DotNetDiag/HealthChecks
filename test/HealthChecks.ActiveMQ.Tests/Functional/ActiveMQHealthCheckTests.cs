using System.Net;

namespace HealthChecks.ActiveMQ.Tests.Functional;

public class activemq_healthcheck_should(ActiveMQContainerFixture activeMQContainerFixture) : IClassFixture<ActiveMQContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_activemq_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddActiveMQ(
                        activeMQContainerFixture.GetConnectionString(),
                        options =>
                        {
                            options.UserName = "admin";
                            options.Password = "admin";
                            options.RequestTimeout = TimeSpan.FromSeconds(5);
                        },
                        tags: ["activemq"],
                        timeout: TimeSpan.FromSeconds(10));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("activemq")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_activemq_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddActiveMQ(
                        "activemq:tcp://127.0.0.1:1",
                        options => options.RequestTimeout = TimeSpan.FromSeconds(1),
                        tags: ["activemq"],
                        timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("activemq")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
