## Valkey Health Check

This health check verifies that Valkey can be reached through the Redis-compatible protocol and can respond to lightweight ping or cluster commands.

### Connection string

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddValkey("localhost:6379");
}
```

### Existing connection multiplexer

Register an `IConnectionMultiplexer` when your application already owns the connection lifetime or needs advanced StackExchange.Redis configuration.

```csharp
using StackExchange.Redis;

void Configure(IServiceCollection services)
{
    services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost:6379"));

    services.AddHealthChecks()
        .AddValkey(sp => sp.GetRequiredService<IConnectionMultiplexer>());
}
```

