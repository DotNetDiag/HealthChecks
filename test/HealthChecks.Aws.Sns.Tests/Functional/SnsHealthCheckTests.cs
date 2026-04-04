using System.Net;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sns.Tests.Functional;

public class aws_sns_healthcheck_should(LocalStackContainerFixture localStackFixture) : IClassFixture<LocalStackContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_aws_sns_topic_is_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;
                            options.AddTopicAndSubscriptions("healthchecks");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("sns")
                });
            }));

        using var server = new TestServer(host.Services);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sns_topic_does_not_exist()
    {
        string connectionString = localStackFixture.GetConnectionString();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;
                            options.AddTopicAndSubscriptions("missing");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("sns")
                });
            }));

        using var server = new TestServer(host.Services);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
