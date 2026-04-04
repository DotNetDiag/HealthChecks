using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace HealthChecks.SignalR.Tests.Functional;

public class signalr_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_signalr_hub_is_available()
    {
        TestServer server = null!;
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                .AddSignalR()
                .Services
                .AddHealthChecks()
                .AddSignalRHub(
                    () => new HubConnectionBuilder()
                            .WithUrl("http://localhost/test", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                            .Build(),
                    tags: ["signalr"]);
            })
            .Configure(app =>
            {

                app
                    .UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("signalr")
                    })
                    .UseRouting()
                    .UseEndpoints(config => config.MapHub<TestHub>("/test"));
            }));

        server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_signalr_hub_is_unavailable()
    {
        TestServer server = null!;
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                .AddSignalR()
                .Services
                .AddHealthChecks()
                .AddSignalRHub(
                    () => new HubConnectionBuilder()
                            .WithUrl("http://localhost/badhub", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                            .Build(),
                    tags: ["signalr"]);
            })
            .Configure(app =>
            {
                app
                    .UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("signalr")
                    })
                    .UseRouting()
                    .UseEndpoints(config => config.MapHub<TestHub>("/test"));
            }));

        server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    private class TestHub : Hub
    {

    }
}
