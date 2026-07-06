## CockroachDB Health Check

This health check verifies that CockroachDB can be reached through its PostgreSQL-compatible SQL endpoint. It can also check a CockroachDB node health endpoint such as `/health?ready=1`.

### SQL connectivity

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddCockroachDb("Host=localhost;Port=26257;Username=root;Database=defaultdb;SSL Mode=Disable");
}
```

### Custom SQL query

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddCockroachDb(
        "Host=localhost;Port=26257;Username=root;Database=defaultdb;SSL Mode=Disable",
        options => options.CommandText = "SELECT count(*) FROM system.namespace");
}
```

### Node readiness endpoint

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddCockroachDb(
        "Host=localhost;Port=26257;Username=root;Database=defaultdb;SSL Mode=Disable",
        options => options.NodeHealthEndpoint = new Uri("http://localhost:8080/health?ready=1"));
}
```

### Existing data source

Register an `NpgsqlDataSource` when your application already owns the data source lifetime or needs advanced Npgsql configuration.

```csharp
using Npgsql;

void Configure(IServiceCollection services)
{
    services.AddNpgsqlDataSource("Host=localhost;Port=26257;Username=root;Database=defaultdb;SSL Mode=Disable");

    services.AddHealthChecks()
        .AddCockroachDb();
}
```
