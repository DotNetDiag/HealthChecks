using System.Net;

namespace HealthChecks.ArangoDb.Tests.Functional;

public class arangodb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_arangodb_is_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddArangoDb(_ => new ArangoDbOptions
                 {
                     HostUri = "http://localhost:8529/",
                     Database = "_system",
                     UserName = "root",
                     Password = "strongArangoDbPassword"
                 }, tags: ["arangodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arangodb")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_arango_are_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddArangoDb(_ => new ArangoDbOptions
                    {
                        HostUri = "http://localhost:8529/",
                        Database = "_system",
                        UserName = "root",
                        Password = "strongArangoDbPassword"
                    }, tags: ["arango"], name: "1")
                    .AddArangoDb(_ => new ArangoDbOptions
                    {
                        HostUri = "http://localhost:8529/",
                        Database = "_system",
                        UserName = "root",
                        Password = "strongArangoDbPassword",
                        IsGenerateJwtTokenBasedOnUserNameAndPassword = true
                    }, tags: ["arango"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arango")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_arango_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddArangoDb(_ => new ArangoDbOptions
                 {
                     HostUri = "http://localhost:8529/",
                     Database = "_system",
                     UserName = "root",
                     Password = "invalid password"
                 }, tags: ["arango"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("arango")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
