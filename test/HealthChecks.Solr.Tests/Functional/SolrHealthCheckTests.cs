using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Solr.Tests.Functional;

public class solr_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_solr_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                .AddSolr("http://localhost:8983/solr", "solrcore", tags: ["solr"]);
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("solr"),
                   ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
               });
           }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_solr_ping_is_disabled()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSolr("http://localhost:8893/solr", "solrcoredown", tags: ["solr"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("solr"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_solr_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSolr("http://200.0.0.100:8893", "core", tags: ["solr"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("solr"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }
}
