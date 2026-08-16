## Cassandra Health Check

This health check verifies the ability to communicate with [Apache Cassandra](https://cassandra.apache.org/). It uses the [CassandraCSharpDriver](https://www.nuget.org/packages/CassandraCSharpDriver) library.

## NuGet

[![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.Cassandra)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.Cassandra)

```shell
dotnet add package DotNetDiag.HealthChecks.Cassandra
```

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `options`: Optional `CassandraHealthCheckOptions` to set a specific keyspace to connect to.
- `name`: The health check name. Default if not specified is `cassandra`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Resolve `ICluster` from the service provider

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddSingleton<ICluster>(sp => Cluster.Builder()
            .AddContactPoint("localhost")
            .Build())
        .AddHealthChecks()
        .AddCassandra(sp => sp.GetRequiredService<ICluster>());
}
```

### Connect to a specific keyspace

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddCassandra(
            sp => sp.GetRequiredService<ICluster>(),
            options: new CassandraHealthCheckOptions { Keyspace = "mykeyspace" });
}
```

### Use a cluster factory inline

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddCassandra(
            _ => Cluster.Builder().AddContactPoint("localhost").Build(),
            name: "cassandra-db",
            tags: ["cassandra", "db"]);
}
```
