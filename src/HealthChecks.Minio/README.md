# MinIO Health Check

This health check verifies the ability to communicate with [MinIO](https://min.io/) by using a configured `IMinioClient`.

## Defaults

By default, the MinIO readiness endpoint is checked. Register a singleton `IMinioClient` or provide one through `clientFactory`.

```csharp
using HealthChecks.Minio;
using Minio;

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IMinioClient>(_ => new MinioClient()
        .WithEndpoint("localhost", 9000)
        .WithCredentials("access-key", "secret-key")
        .WithSSL(false)
        .Build());

    services
        .AddHealthChecks()
        .AddMinio();
}
```

## MinIO probe endpoints

Use `HealthCheckType` to select the MinIO endpoint probe. Supported values are `Ready`, `Live`, `Cluster`, and `ClusterRead`.

```csharp
using HealthChecks.Minio;
using Minio;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddMinio(
            clientFactory: _ => new MinioClient()
                .WithEndpoint("localhost", 9000)
                .WithCredentials("access-key", "secret-key")
                .WithSSL(false)
                .Build(),
            optionsFactory: _ => new MinioHealthCheckOptions
            {
                HealthCheckType = MinioHealthCheckType.Cluster
            });
}
```

If the health probe is exposed through a different route or needs query parameters, set `HealthEndpointUri`.

```csharp
using HealthChecks.Minio;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddMinio(optionsFactory: _ => new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.Cluster,
            HealthEndpointUri = new Uri("https://minio.example.com/minio/health/cluster?maintenance=true")
        });
}
```

## Bucket check

Use `BucketExists` to check whether a bucket is available through the MinIO S3-compatible API.

```csharp
using HealthChecks.Minio;
using Minio;

void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IMinioClient>(_ => new MinioClient()
        .WithEndpoint("localhost", 9000)
        .WithCredentials("access-key", "secret-key")
        .WithSSL(false)
        .Build());

    services
        .AddHealthChecks()
        .AddMinio(optionsFactory: _ => new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.BucketExists,
            BucketName = "bucket-name"
        });
}
```

## Service check

Use `ListBuckets` to verify authenticated service reachability without requiring a specific bucket.

```csharp
using HealthChecks.Minio;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddMinio(optionsFactory: _ => new MinioHealthCheckOptions
        {
            HealthCheckType = MinioHealthCheckType.ListBuckets
        });
}
```

## Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide an `IMinioClient` instance.
- `optionsFactory`: A factory method to provide a `MinioHealthCheckOptions` instance.
- `name`: The health check name. The default is `minio`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
