# Milvus Health Check

This health check verifies the ability to communicate with a [Milvus](https://milvus.io/) service by using `Milvus.Client`.

This package is published as prerelease while `Milvus.Client` is prerelease-only.

## Builder Extension

Register a `MilvusClient` in the service container or provide one with `clientFactory`.

```csharp
using Milvus.Client;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddMilvus(clientFactory: _ => new MilvusClient(new Uri("http://localhost:19530")));
}
```

You can also register a singleton client and let the health check resolve it from the service provider.

```csharp
using Milvus.Client;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddSingleton(new MilvusClient(new Uri("http://localhost:19530")))
        .AddHealthChecks()
        .AddMilvus();
}
```

The health check calls `MilvusClient.HealthAsync`. An unhealthy Milvus response is reported with the configured failure status and includes the response error code and message in the health check data.

Like all `IHealthChecksBuilder` extensions, the following parameters can be overridden:

- `clientFactory`: A factory method to provide a `MilvusClient` instance.
- `name`: The health check name. The default is `milvus`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. The default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

[<<](../../README.md)
