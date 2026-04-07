## Apache IoTDB Health Check

This health check verifies the ability to communicate with [Apache IoTDB](https://iotdb.apache.org/). It uses the [Apache.IoTDB.Data](https://www.nuget.org/packages/Apache.IoTDB.Data) library.

## NuGet

[![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.IoTDB)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.IoTDB)

```shell
dotnet add package DotNetDiag.HealthChecks.IoTDB
```

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `iotdb`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Use a connection string

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddIoTDB("DataSource=localhost;Port=6667;Username=root;Password=root");
}
```

### Use `IoTDBHealthCheckOptions` for advanced configuration

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddIoTDB(new IoTDBHealthCheckOptions
        {
            ConnectionString = "DataSource=localhost;Port=6667;Username=root;Password=root",
            EnableRpcCompression = true
        });
}
```
