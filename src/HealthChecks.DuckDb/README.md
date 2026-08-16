## DuckDB Health Check

This health check verifies that a DuckDB database can be opened and can execute a lightweight SQL query.

### Defaults

By default, the package opens a DuckDB connection and executes `SELECT 1;`.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddDuckDb("Data Source=analytics.duckdb");
}
```

Use DuckDB's in-memory database when you only need to validate that the embedded DuckDB engine can start.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddDuckDb("Data Source=:memory:");
}
```

### Custom query

Use a custom query when your application needs to validate a specific table, view, extension, or permission.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddDuckDb(
        "Data Source=analytics.duckdb",
        healthQuery: "SELECT COUNT(*) FROM healthcheck_marker");
}
```

### Advanced options

Use `DuckDbHealthCheckOptions` when you need to customize the connection before it opens or build a custom health check result from the query result.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddDuckDb(new DuckDbHealthCheckOptions("Data Source=analytics.duckdb")
    {
        CommandText = "SELECT 1;",
        HealthCheckResultBuilder = result => HealthCheckResult.Healthy($"DuckDB returned {result}")
    });
}
```

### Target frameworks

DuckDB's .NET provider targets .NET 8 and later. This health check package targets .NET 8 and .NET 10 and references `DuckDB.NET.Data.Full` so supported DuckDB native binaries are available with the package.
