using Google.Cloud.Storage.V1;
using NSubstitute;

namespace HealthChecks.Gcp.CloudStorage.Tests;

public class CloudStorageConformanceTests : ConformanceTests<StorageClient, CloudStorageHealthCheck, CloudStorageHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, StorageClient>? clientFactory = null,
        Func<IServiceProvider, CloudStorageHealthCheckOptions>? optionsFactory = null,
        string? healthCheckName = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        return builder.AddCloudStorage(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override StorageClient CreateClientForNonExistingEndpoint()
        => Substitute.For<StorageClient>();

    protected override CloudStorageHealthCheck CreateHealthCheck(StorageClient client, CloudStorageHealthCheckOptions? options)
        => new(client, options);

    protected override CloudStorageHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
