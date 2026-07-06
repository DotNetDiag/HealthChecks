using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HealthChecks.CockroachDb.Tests.Functional;

public class cockroachdb_healthcheck_should(CockroachDbContainerFixture cockroachDbContainerFixture) : IClassFixture<CockroachDbContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_cockroachdb_sql_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(cockroachDbContainerFixture.GetConnectionString(), tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_cockroachdb_node_health_endpoint_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(cockroachDbContainerFixture.GetNodeHealthEndpoint(), tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_sql_and_node_health_endpoint_are_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(
                        cockroachDbContainerFixture.GetConnectionString(),
                        options => options.NodeHealthEndpoint = cockroachDbContainerFixture.GetNodeHealthEndpoint(),
                        tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(
                        cockroachDbContainerFixture.GetConnectionString(),
                        options => options.CommandText = "SELECT 1 FROM InvalidDB",
                        tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_cockroachdb_sql_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(
                        "Host=127.0.0.1;Port=1;Username=root;Database=defaultdb;SSL Mode=Disable",
                        tags: ["cockroachdb"],
                        timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_node_health_endpoint_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCockroachDb(
                        new Uri("http://127.0.0.1:1/health?ready=1"),
                        tags: ["cockroachdb"],
                        timeout: TimeSpan.FromSeconds(3));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_cockroachdb_is_available_by_iServiceProvider_registered()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddNpgsqlDataSource(cockroachDbContainerFixture.GetConnectionString());
                services.AddHealthChecks()
                    .AddCockroachDb(tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task unhealthy_check_log_detailed_messages()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddLogging(b =>
                        b.ClearProviders()
                            .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>()))
                    .AddHealthChecks()
                    .AddCockroachDb(
                        cockroachDbContainerFixture.GetConnectionString(),
                        options => options.CommandText = "SELECT 1 FROM InvalidDB",
                        tags: ["cockroachdb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cockroachdb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        var testLoggerProvider = (TestLoggerProvider)server.Services.GetRequiredService<ILoggerProvider>();
        var logger = testLoggerProvider.GetLogger("Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService");

        logger.ShouldNotBeNull();
        logger?.EventLog[0].Item2.ShouldNotContain("with message '(null)'");
    }
}
