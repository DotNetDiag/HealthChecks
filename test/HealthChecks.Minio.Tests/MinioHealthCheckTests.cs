using System.Net;
using HealthChecks.Minio.Tests.Helpers;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Result;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.Minio.Tests;

public class miniohealthcheck_should
{
    private const string BucketName = "unit-test-bucket";
    private const string HealthCheckName = "unit-test-check";

    [Theory]
    [InlineData(MinioHealthCheckType.Ready, "/minio/health/ready")]
    [InlineData(MinioHealthCheckType.Live, "/minio/health/live")]
    [InlineData(MinioHealthCheckType.Cluster, "/minio/health/cluster")]
    [InlineData(MinioHealthCheckType.ClusterRead, "/minio/health/cluster/read")]
    public async Task call_expected_minio_health_endpoint(MinioHealthCheckType healthCheckType, string expectedPath)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using IMinioClient client = CreateMinioClient(handler);
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = healthCheckType
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsolutePath.ShouldBe(expectedPath);
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task use_custom_health_endpoint_uri_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using IMinioClient client = CreateMinioClient(handler);
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.Cluster,
            HealthEndpointUri = new Uri("http://minio.example.com/minio/health/cluster?maintenance=true")
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("http://minio.example.com/minio/health/cluster?maintenance=true");
    }

    [Fact]
    public async Task return_failure_status_when_health_endpoint_is_not_successful()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        using IMinioClient client = CreateMinioClient(handler);
        var healthCheck = new MinioHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("MinIO health endpoint returned HTTP 503 (Service Unavailable).");
    }

    [Fact]
    public async Task return_healthy_when_bucket_exists()
    {
        using var tokenSource = new CancellationTokenSource();
        IMinioClient client = Substitute.For<IMinioClient>();
        client
            .BucketExistsAsync(Arg.Any<BucketExistsArgs>(), tokenSource.Token)
            .Returns(Task.FromResult(true));
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.BucketExists,
            BucketName = BucketName
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck), tokenSource.Token);

        await client
            .Received(1)
            .BucketExistsAsync(Arg.Any<BucketExistsArgs>(), tokenSource.Token);
        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_failure_status_when_bucket_does_not_exist()
    {
        IMinioClient client = Substitute.For<IMinioClient>();
        client
            .BucketExistsAsync(Arg.Any<BucketExistsArgs>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.BucketExists,
            BucketName = BucketName
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Bucket 'unit-test-bucket' does not exist.");
    }

    [Fact]
    public async Task return_failure_status_when_bucket_name_is_not_configured()
    {
        IMinioClient client = Substitute.For<IMinioClient>();
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.BucketExists
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        await client
            .DidNotReceiveWithAnyArgs()
            .BucketExistsAsync(default!, default);
        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("BucketName must be configured.");
    }

    [Fact]
    public async Task return_healthy_when_buckets_can_be_listed()
    {
        using var tokenSource = new CancellationTokenSource();
        IMinioClient client = Substitute.For<IMinioClient>();
        client
            .ListBucketsAsync(tokenSource.Token)
            .Returns(Task.FromResult(new ListAllMyBucketsResult()));
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.ListBuckets
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck), tokenSource.Token);

        await client
            .Received(1)
            .ListBucketsAsync(tokenSource.Token);
        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_failure_status_when_list_buckets_fails()
    {
        var exception = new InvalidOperationException("Unable to list buckets.");
        IMinioClient client = Substitute.For<IMinioClient>();
        client
            .ListBucketsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        var healthCheck = new MinioHealthCheck(client, new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.ListBuckets
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Exception.ShouldBe(exception);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        using IMinioClient client = CreateMinioClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddMinio(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        var service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("MinIO health endpoint returned HTTP 503 (Service Unavailable).");
    }

    private static IMinioClient CreateMinioClient(RecordingHttpMessageHandler handler)
    {
        return new MinioClient()
            .WithEndpoint("localhost", 9000)
            .WithCredentials("access-key", "secret-key")
            .WithSSL(false)
            .WithHttpClient(new HttpClient(handler), disposeHttpClient: true)
            .Build();
    }

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
