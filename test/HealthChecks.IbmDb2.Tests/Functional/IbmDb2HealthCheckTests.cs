using System.Net;

namespace HealthChecks.IbmDb2.Tests.Functional;

public class ibm_db2_healthcheck_should(IbmDb2ContainerFixture ibmDb2ContainerFixture) : IClassFixture<IbmDb2ContainerFixture>
{
    [LinuxFact]
    public async Task be_healthy_when_ibm_db2_server_is_available()
    {
        string connectionString = ibmDb2ContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIbmDb2(connectionString, tags: ["ibmdb2"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmdb2")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [LinuxFact]
    public async Task be_unhealthy_when_ibm_db2_server_is_unavailable()
    {
        const string connectionString = "Server=127.0.0.1:1;Database=testdb;UID=db2inst1;PWD=password;Connect Timeout=1;";

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIbmDb2(connectionString, tags: ["ibmdb2"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmdb2")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [LinuxFact]
    public async Task be_unhealthy_when_ibm_db2_server_is_unavailable_using_options()
    {
        var healthCheckOptions = new IbmDb2HealthCheckOptions
        {
            ConnectionString = "Server=127.0.0.1:1;Database=testdb;UID=db2inst1;PWD=password;Connect Timeout=1;"
        };

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIbmDb2(healthCheckOptions, tags: ["ibmdb2"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmdb2")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [LinuxFact]
    public async Task be_unhealthy_when_query_is_not_valid()
    {
        string connectionString = ibmDb2ContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIbmDb2(connectionString, "SELECT 1 FROM InvalidDb2Table", tags: ["ibmdb2"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmdb2")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [LinuxFact]
    public async Task be_healthy_with_connection_string_factory_when_ibm_db2_server_is_available()
    {
        bool factoryCalled = false;
        string connectionString = ibmDb2ContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIbmDb2(_ =>
                    {
                        factoryCalled = true;
                        return connectionString;
                    }, tags: ["ibmdb2"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ibmdb2")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }
}
