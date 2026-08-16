using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Firebird.Tests.Functional;

public class firebird_healthcheck_should(FirebirdContainerFixture firebirdContainerFixture) : IClassFixture<FirebirdContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_firebird_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFirebird(firebirdContainerFixture.GetConnectionString(), tags: ["firebird"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("firebird"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_firebird_is_unavailable()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFirebird(
                        "Database=127.0.0.1/1:healthchecks.fdb;User=SYSDBA;Password=masterkey;Connection Timeout=1",
                        tags: ["firebird"],
                        timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("firebird"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFirebird(firebirdContainerFixture.GetConnectionString(), "SELECT 1 FROM NOT_VALID_TABLE", tags: ["firebird"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("firebird")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_with_connection_string_factory_when_firebird_is_available()
    {
        bool factoryCalled = false;
        string connectionString = firebirdContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFirebird(_ =>
                    {
                        factoryCalled = true;
                        return connectionString;
                    }, tags: ["firebird"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("firebird")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }
}
