using System.Net;

namespace HealthChecks.DuckDb.Tests.Functional;

public class duckdb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_duckdb_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddDuckDb("Data Source=:memory:", tags: ["duckdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("duckdb")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_when_duckdb_database_can_not_be_opened()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddDuckDb(DuckDbConformanceTests.CreateConnectionStringForNonExistingDatabase(), tags: ["duckdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("duckdb")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_sql_query_is_not_valid()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddDuckDb("Data Source=:memory:", "SELECT * FROM InvalidDuckDbTable", tags: ["duckdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("duckdb")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_with_connection_string_factory_when_duckdb_is_available()
    {
        bool factoryCalled = false;

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddDuckDb(_ =>
                    {
                        factoryCalled = true;
                        return "Data Source=:memory:";
                    }, tags: ["duckdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("duckdb")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }
}
