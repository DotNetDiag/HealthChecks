using System.Net;

namespace HealthChecks.OpenSearch.Tests.Functional;

public class OpenSearchAuthenticationTests(OpenSearchSecuredContainerFixture openSearchContainerFixture)
    : IClassFixture<OpenSearchSecuredContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_opensearch_is_using_valid_user_and_password()
    {
        var connectionString = openSearchContainerFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddOpenSearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseBasicAuthentication(
                            OpenSearchSecuredContainerFixture.AdminUserName,
                            OpenSearchSecuredContainerFixture.AdminPassword);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
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
}
