using System.Net;

namespace HealthChecks.Artemis.Tests.Functional;

public class artemis_healthcheck_should(ArtemisContainerFixture artemisContainerFixture) : IClassFixture<ArtemisContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_artemis_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddArtemis(
                        artemisContainerFixture.GetConnectionString(),
                        options =>
                        {
                            options.UserName = "artemis";
                            options.Password = "artemis";
                            options.RequestTimeout = TimeSpan.FromSeconds(5);
                        },
                        tags: ["artemis"],
                        timeout: TimeSpan.FromSeconds(10));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("artemis")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_artemis_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddArtemis(
                        "amqp://127.0.0.1:1",
                        options => options.RequestTimeout = TimeSpan.FromSeconds(1),
                        tags: ["artemis"],
                        timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("artemis")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
