# OpenSearch Health Check

This health check verifies the ability to communicate with an [OpenSearch](https://opensearch.org/) cluster.

## Example Usage

```csharp
services.AddHealthChecks()
    .AddOpenSearch("http://localhost:9200");
```

To use the cluster health API instead of the ping API:

```csharp
services.AddHealthChecks()
    .AddOpenSearch("http://localhost:9200", useClusterHealthApi: true);
```

For secured clusters, configure the underlying OpenSearch client settings through options:

```csharp
services.AddHealthChecks()
    .AddOpenSearch(options =>
    {
        options.UseServer("https://localhost:9200");
        options.UseBasicAuthentication("admin", "password");
        options.UseCertificateValidationCallback(delegate
        {
            return true;
        });
        options.RequestTimeout = TimeSpan.FromSeconds(10);
    });
```

To reuse an existing client:

```csharp
services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(new Uri("http://localhost:9200")));

services.AddHealthChecks()
    .AddOpenSearch();
```
