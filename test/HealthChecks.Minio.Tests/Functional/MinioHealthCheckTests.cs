using System.Net;
using Minio;
using Minio.DataModel.Args;

namespace HealthChecks.Minio.Tests.Functional;

public class minio_healthcheck_should(MinioContainerFixture minioContainerFixture) : IClassFixture<MinioContainerFixture>
{
    private const string BucketName = "healthchecks-minio";

    [Fact]
    public async Task be_healthy_when_minio_is_ready_using_client_factory()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddMinio(
                        clientFactory: _ => minioContainerFixture.CreateClient(),
                        optionsFactory: _ => new MinioHealthCheckOptions
                        {
                            HealthCheckType = MinioHealthCheckType.Ready
                        },
                        tags: new[] { "minio" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("minio")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_bucket_exists_using_registered_client()
    {
        using IMinioClient setupClient = minioContainerFixture.CreateClient();
        await setupClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(_ => minioContainerFixture.CreateClient())
                    .AddHealthChecks()
                    .AddMinio(
                        optionsFactory: _ => new MinioHealthCheckOptions
                        {
                            HealthCheckType = MinioHealthCheckType.BucketExists,
                            BucketName = BucketName
                        },
                        tags: new[] { "minio" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("minio")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_bucket_is_missing()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(_ => minioContainerFixture.CreateClient())
                    .AddHealthChecks()
                    .AddMinio(
                        optionsFactory: _ => new MinioHealthCheckOptions
                        {
                            HealthCheckType = MinioHealthCheckType.BucketExists,
                            BucketName = "missing-bucket"
                        },
                        tags: new[] { "minio" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("minio")
                });
            }));

        TestServer server = host.GetTestServer();

        using HttpResponseMessage response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
