## Firebird Health Check

This health check verifies that a Firebird database can be reached and can execute a lightweight SQL query.

### Defaults

By default, the package opens a Firebird connection and executes `SELECT 1 FROM RDB$DATABASE`.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddFirebird("Database=localhost:/firebird/data/app.fdb;User=SYSDBA;Password=masterkey");
}
```

### Custom query

Use a custom query when your application needs to validate a specific database object or permission.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddFirebird(
        "Database=localhost:/firebird/data/app.fdb;User=SYSDBA;Password=masterkey",
        healthQuery: "SELECT COUNT(*) FROM RDB$RELATIONS");
}
```

### Advanced options

Use `FirebirdHealthCheckOptions` when you need to customize the connection before it opens or build a custom health check result from the query result.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddFirebird(new FirebirdHealthCheckOptions("Database=localhost:/firebird/data/app.fdb;User=SYSDBA;Password=masterkey")
    {
        CommandText = "SELECT 1 FROM RDB$DATABASE",
        HealthCheckResultBuilder = result => HealthCheckResult.Healthy($"Firebird returned {result}")
    });
}
```
