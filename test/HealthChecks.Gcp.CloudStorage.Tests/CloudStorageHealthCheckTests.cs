using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.Gcp.CloudStorage.Tests;

public class cloudstoragehealthcheck_should
{
    private const string BucketName = "unit-test-bucket";
    private const string HealthCheckName = "unit-test-check";
    private const string ProjectId = "unit-test-project";

    private readonly StorageClient _storageClient;
    private readonly CloudStorageHealthCheckOptions _options;
    private readonly CloudStorageHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public cloudstoragehealthcheck_should()
    {
        _storageClient = Substitute.For<StorageClient>();
        _options = new CloudStorageHealthCheckOptions();
        _healthCheck = new CloudStorageHealthCheck(_storageClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };
    }

    [Fact]
    public async Task return_unhealthy_when_neither_project_nor_bucket_is_configured()
    {
        var actual = await _healthCheck.CheckHealthAsync(_context);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("ProjectId or BucketName must be configured.");
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();
        var pageable = Substitute.For<PagedAsyncEnumerable<Buckets, Bucket>>();
        var enumerator = Substitute.For<IAsyncEnumerator<Bucket>>();

        _storageClient
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1))
            .Returns(pageable);

        pageable
            .GetAsyncEnumerator(tokenSource.Token)
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .Returns(new ValueTask<bool>(false));

        _options.ProjectId = ProjectId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _storageClient
            .Received(1)
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1));

        await _storageClient
            .DidNotReceiveWithAnyArgs()
            .GetBucketAsync(default!, default, default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_bucket()
    {
        using var tokenSource = new CancellationTokenSource();
        var getBucketOptions = new GetBucketOptions();

        _storageClient
            .GetBucketAsync(BucketName, getBucketOptions, tokenSource.Token)
            .Returns(Task.FromResult(new Bucket()));

        _options.BucketName = BucketName;
        _options.GetBucketOptions = getBucketOptions;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _storageClient
            .Received(1)
            .GetBucketAsync(BucketName, getBucketOptions, tokenSource.Token);

        _storageClient
            .DidNotReceiveWithAnyArgs()
            .ListBucketsAsync(default!, default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task prefer_bucket_check_when_project_and_bucket_are_configured()
    {
        using var tokenSource = new CancellationTokenSource();

        _storageClient
            .GetBucketAsync(BucketName, default, tokenSource.Token)
            .Returns(Task.FromResult(new Bucket()));

        _options.ProjectId = ProjectId;
        _options.BucketName = BucketName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _storageClient
            .Received(1)
            .GetBucketAsync(BucketName, default, tokenSource.Token);

        _storageClient
            .DidNotReceiveWithAnyArgs()
            .ListBucketsAsync(default!, default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_service()
    {
        using var tokenSource = new CancellationTokenSource();
        var pageable = Substitute.For<PagedAsyncEnumerable<Buckets, Bucket>>();
        var enumerator = Substitute.For<IAsyncEnumerator<Bucket>>();
        var exception = new InvalidOperationException("Unable to list buckets.");

        _storageClient
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1))
            .Returns(pageable);

        pageable
            .GetAsyncEnumerator(tokenSource.Token)
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(exception);

        _options.ProjectId = ProjectId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _storageClient
            .Received(1)
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1));

        pageable
            .Received(1)
            .GetAsyncEnumerator(tokenSource.Token);

        await enumerator
            .Received(1)
            .MoveNextAsync();

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception.ShouldBe(exception);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_bucket()
    {
        using var tokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("Bucket not found.");

        _storageClient
            .GetBucketAsync(BucketName, default, tokenSource.Token)
            .ThrowsAsync(exception);

        _options.BucketName = BucketName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _storageClient
            .Received(1)
            .GetBucketAsync(BucketName, default, tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception.ShouldBe(exception);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var pageable = Substitute.For<PagedAsyncEnumerable<Buckets, Bucket>>();
        var enumerator = Substitute.For<IAsyncEnumerator<Bucket>>();

        using var provider = new ServiceCollection()
            .AddSingleton(_storageClient)
            .AddLogging()
            .AddHealthChecks()
            .AddCloudStorage(
                optionsFactory: _ => new CloudStorageHealthCheckOptions { ProjectId = ProjectId },
                name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _storageClient
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1))
            .Returns(pageable);

        pageable
            .GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(new InvalidOperationException("Unable to list buckets."));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        _storageClient
            .Received(1)
            .ListBucketsAsync(
                ProjectId,
                Arg.Is<ListBucketsOptions>(options => options.PageSize == 1));

        pageable
            .Received(1)
            .GetAsyncEnumerator(Arg.Any<CancellationToken>());

        await enumerator
            .Received(1)
            .MoveNextAsync();

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice_for_bucket()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_storageClient)
            .AddLogging()
            .AddHealthChecks()
            .AddCloudStorage(
                optionsFactory: _ => new CloudStorageHealthCheckOptions { BucketName = BucketName },
                name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _storageClient
            .GetBucketAsync(BucketName, default, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Bucket not found."));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        await _storageClient
            .Received(1)
            .GetBucketAsync(BucketName, default, Arg.Any<CancellationToken>());

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception.ShouldBeOfType<InvalidOperationException>();
    }
}
