using System.Net;

namespace HealthChecks.OpenSearch.Tests.Functional;

public class opensearch_healthcheck_should(OpenSearchContainerFixture openSearchContainerFixture) : IClassFixture<OpenSearchContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_opensearch_is_available()
    {
        var connectionString = openSearchContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddOpenSearch(connectionString, tags: ["opensearch"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("opensearch")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_opensearch_cluster_health_is_available()
    {
        var connectionString = openSearchContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddOpenSearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseClusterHealthApi = true;
                    }, tags: ["opensearch"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("opensearch")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_opensearch_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddOpenSearch(options =>
                    {
                        options.UseServer("http://127.0.0.1:1");
                        options.RequestTimeout = TimeSpan.FromSeconds(1);
                    }, tags: ["opensearch"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("opensearch")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
