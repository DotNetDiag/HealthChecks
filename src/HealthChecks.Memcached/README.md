## Memcached Health Check

This health check verifies that Memcached can store, read, and remove a short-lived cache item.

### Basic usage

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddMemcached("localhost");
}
```

### Custom port

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddMemcached("memcached", 11211);
}
```

### Advanced client options

Configure the underlying Enyim Memcached client options when the check needs multiple servers or custom socket settings.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddMemcached(options =>
    {
        options.AddServer("memcached-a", 11211);
        options.AddServer("memcached-b", 11211);
        options.ClientOptions.SocketPool.ConnectionTimeout = TimeSpan.FromSeconds(3);
    });
}
```

### Existing client

Use an existing `IMemcachedClient` when your application already owns the client lifetime or needs advanced Enyim configuration.

```csharp
using Enyim.Caching;

void Configure(IServiceCollection services)
{
    services.AddSingleton<IMemcachedClient>(/* create or register your Enyim client */);

    services.AddHealthChecks()
        .AddMemcached();
}
```
