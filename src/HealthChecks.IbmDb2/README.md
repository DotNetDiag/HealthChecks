## IBM Db2 Health Check

This health check verifies that an IBM Db2 database can be reached and can execute a lightweight query.

### Defaults

By default, the health check opens a Db2 connection and executes `SELECT 1 FROM SYSIBM.SYSDUMMY1`.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddIbmDb2("Server=db2.example.com:50000;Database=testdb;UID=db2inst1;PWD=password;");
}
```

### Custom query and connection configuration

Use a custom query or configure the `DB2Connection` before it opens when your environment needs provider-specific settings.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddIbmDb2(
        "Server=db2.example.com:50000;Database=testdb;UID=db2inst1;PWD=password;",
        healthQuery: "SELECT CURRENT DATE FROM SYSIBM.SYSDUMMY1",
        configure: connection =>
        {
            // Adjust DB2Connection settings here before the connection opens.
        });
}
```

### Platform-specific IBM provider packages

IBM ships the Db2 .NET provider as platform-specific NuGet packages. This health check targets .NET 8 and .NET 10 and references the Linux AMD64 provider package used by the repository CI. Applications running on other platforms should follow IBM's package guidance for the matching provider package.
