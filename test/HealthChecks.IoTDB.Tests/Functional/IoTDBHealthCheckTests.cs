using System.Net;

namespace HealthChecks.IoTDB.Tests.Functional;

public class iotdb_healthcheck_should(IoTDBContainerFixture ioTDBFixture) : IClassFixture<IoTDBContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_iotdb_is_available()
    {
        var connectionString = ioTDBFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIoTDB(connectionString, tags: ["iotdb"]);
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("iotdb")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_iotdb_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddIoTDB("DataSource=200.0.0.1;Port=6667;Username=root;Password=root", tags: ["iotdb"], timeout: TimeSpan.FromSeconds(15));
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("iotdb")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
