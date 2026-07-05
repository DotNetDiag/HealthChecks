## Google Cloud Storage Health Check

This health check verifies the ability to communicate with [Google Cloud Storage](https://cloud.google.com/storage). It uses the provided [StorageClient](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Storage.V1/latest/Google.Cloud.Storage.V1.StorageClient) to list buckets for a project or fetch metadata for a configured bucket.

### Defaults

By default, the `StorageClient` instance is resolved from the service provider. Configure either `ProjectId` for a service-level check or `BucketName` for a bucket-level check.

```csharp
using Google.Cloud.Storage.V1;
using HealthChecks.Gcp.CloudStorage;

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(_ => StorageClient.Create());

    services
        .AddHealthChecks()
        .AddCloudStorage(optionsFactory: _ => new CloudStorageHealthCheckOptions
        {
            ProjectId = "my-gcp-project"
        });
}
```

### Bucket check

When `BucketName` is set, the health check fetches that bucket's metadata. `BucketName` takes precedence over `ProjectId` when both values are configured.

```csharp
using Google.Cloud.Storage.V1;
using HealthChecks.Gcp.CloudStorage;

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(_ => StorageClient.Create());

    services
        .AddHealthChecks()
        .AddCloudStorage(optionsFactory: _ => new CloudStorageHealthCheckOptions
        {
            BucketName = "my-bucket"
        });
}
```

### Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide a `StorageClient` instance.
- `optionsFactory`: A factory method to provide a `CloudStorageHealthCheckOptions` instance. It allows specifying `ProjectId`, `BucketName`, `GetBucketOptions`, and `ListBucketsOptions`.
- `name`: The health check name. The default is `gcp_cloud_storage`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
