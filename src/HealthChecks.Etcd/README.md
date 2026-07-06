## etcd Health Check

This health check verifies that an etcd v3 server can be reached and responds to a status request.

### Defaults

By default, the package creates an `EtcdClient` for the configured endpoint and calls `StatusAsync`.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddEtcd("http://localhost:2379");
}
```

### Custom client settings

Configure `EtcdHealthCheckOptions` when the endpoint uses a non-default port or custom gRPC channel settings.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddEtcd(
        "https://etcd.example.com",
        options =>
        {
            options.Port = 2379;
            options.ConfigureSslOptions = ssl => ssl.TargetHost = "etcd.example.com";
        });
}
```

### Existing client

Register an `EtcdClient` when your application already owns the client lifetime or needs advanced authentication configuration.

```csharp
using dotnet_etcd;

void Configure(IServiceCollection services)
{
    services.AddSingleton(new EtcdClient("http://localhost:2379"));

    services.AddHealthChecks()
        .AddEtcd(sp => sp.GetRequiredService<EtcdClient>());
}
```
