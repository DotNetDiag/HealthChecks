using System.Net;
using DotPulsar;
using DotPulsar.Abstractions;

namespace HealthChecks.Apache.Pulsar.Tests.Functional;

public class pulsar_healthcheck_should(PulsarContainerFixture pulsarContainerFixture) : IClassFixture<PulsarContainerFixture>
{
    private const string Topic = "persistent://public/default/healthchecks";

    [Fact]
    public async Task be_healthy_if_pulsar_broker_is_available()
    {
        await using IPulsarClient client = PulsarClient.Builder()
            .ServiceUrl(new Uri(pulsarContainerFixture.GetPulsarBrokerUrl()))
            .Build();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(client)
                    .AddHealthChecks()
                    .AddPulsar(
                        optionsFactory: _ => new PulsarHealthCheckOptions { Topic = Topic },
                        tags: ["pulsar"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public class pulsar_unavailable_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_pulsar_broker_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(PulsarTestClientFactory.CreateUnavailableClient())
                    .AddHealthChecks()
                    .AddPulsar(
                        optionsFactory: _ => new PulsarHealthCheckOptions(),
                        tags: ["pulsar"],
                        timeout: TimeSpan.FromSeconds(5));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("pulsar")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
