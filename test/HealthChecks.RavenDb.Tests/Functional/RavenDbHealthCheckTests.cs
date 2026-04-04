using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.RavenDb.Tests.Functional;

public class ravendb_healthcheck_should(RavenDbContainerFixture ravenDbFixture) : IClassFixture<RavenDbContainerFixture>
{
    private readonly string[] _urls = [ravenDbFixture.GetConnectionString()];

    [Fact]
    public async Task be_healthy_if_ravendb_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ => _.Urls = _urls, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_timeout_is_too_low()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                        _.RequestTimeout = TimeSpan.FromMilliseconds(0.001);
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_not_available()
    {
        var connectionString = "http://localhost:9999";

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ => _.Urls = [connectionString], tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "ThisDatabaseReallyDoesnExist";
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }
}
