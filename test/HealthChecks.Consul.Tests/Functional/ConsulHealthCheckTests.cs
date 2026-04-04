using System.Net;

namespace HealthChecks.Consul.Tests.Functional;

public class consul_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_consul_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddConsul(setup =>
                   {
                       setup.HostName = "localhost";
                       setup.Port = 8500;
                       setup.RequireHttps = false;
                   }, tags: ["consul"]);
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("consul")
               });
           }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_consul_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddConsul(setup =>
                    {
                        setup.HostName = "non-existing-host";
                        setup.Port = 8500;
                        setup.RequireHttps = false;
                    }, tags: ["consul"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("consul")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
