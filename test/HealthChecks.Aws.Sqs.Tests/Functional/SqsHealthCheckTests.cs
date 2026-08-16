using System.Net;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sqs.Tests.Functional;

public class aws_sqs_healthcheck_should(LocalStackContainerFixture localStackFixture) : IClassFixture<LocalStackContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_aws_sqs_queue_is_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;
                            options.AddQueue("healthchecks");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("sqs")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sqs_queue_does_not_exist()
    {
        string connectionString = localStackFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;
                            options.AddQueue("missing");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("sqs")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
