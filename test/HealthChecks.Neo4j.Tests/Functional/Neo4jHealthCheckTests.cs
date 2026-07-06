using System.Net;
using Neo4j.Driver;

namespace HealthChecks.Neo4j.Tests.Functional;

public class neo4j_healthcheck_should(Neo4jContainerFixture neo4jFixture) : IClassFixture<Neo4jContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_neo4j_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(_ => GraphDatabase.Driver(neo4jFixture.GetConnectionString(), AuthTokens.None))
                    .AddHealthChecks()
                    .AddNeo4j(tags: ["neo4j"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("neo4j")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_neo4j_database_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(_ => GraphDatabase.Driver(neo4jFixture.GetConnectionString(), AuthTokens.None))
                    .AddHealthChecks()
                    .AddNeo4j(databaseNameFactory: _ => "neo4j", tags: ["neo4j"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("neo4j")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_neo4j_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(_ => GraphDatabase.Driver(
                        "bolt://localhost:1",
                        AuthTokens.None,
                        config => config.WithConnectionTimeout(TimeSpan.FromMilliseconds(100))))
                    .AddHealthChecks()
                    .AddNeo4j(tags: ["neo4j"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("neo4j")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
