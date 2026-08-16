using System.Net;

namespace HealthChecks.Gremlin.Tests.Functional;

public class gremlin_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_gremlin_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => new GremlinOptions
                 {
                     Hostname = "localhost",
                     Port = 8182,
                     EnableSsl = false
                 }, tags: ["gremlin"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_gremlin_are_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => new GremlinOptions
                 {
                     Hostname = "localhost",
                     Port = 8182,
                     EnableSsl = false
                 }, tags: ["gremlin"], name: "1")
                 .AddGremlin(_ => new GremlinOptions
                 {
                     Hostname = "localhost",
                     Port = 8182,
                     EnableSsl = false
                 }, tags: ["gremlin"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_gremlin_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddGremlin(_ => new GremlinOptions
                 {
                     Hostname = "wronghost",
                     Port = 8182,
                     EnableSsl = false
                 }, tags: ["gremlin"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("gremlin")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
