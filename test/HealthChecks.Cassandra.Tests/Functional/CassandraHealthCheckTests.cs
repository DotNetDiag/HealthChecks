using System.Net;
using Cassandra;

namespace HealthChecks.Cassandra.Tests.Functional;

public class cassandra_healthcheck_should(CassandraContainerFixture cassandraFixture) : IClassFixture<CassandraContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_cassandra_is_available()
    {
        var cluster = Cluster.Builder()
            .AddContactPoint(cassandraFixture.GetContactPoint())
            .WithPort(cassandraFixture.GetPort())
            .Build();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCassandra(_ => cluster, tags: ["cassandra"]);
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("cassandra")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_cassandra_is_not_available()
    {
        var cluster = Cluster.Builder()
            .AddContactPoint("200.0.0.1")
            .WithPort(9042)
            .WithSocketOptions(new SocketOptions().SetConnectTimeoutMillis(2000).SetReadTimeoutMillis(2000))
            .Build();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddCassandra(_ => cluster, tags: ["cassandra"], timeout: TimeSpan.FromSeconds(15));
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("cassandra")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
