## Neo4j Health Check

This health check verifies the ability to communicate with [Neo4j](https://neo4j.com/). It uses the provided [Neo4j .NET driver](https://github.com/neo4j/neo4j-dotnet-driver) to verify connectivity or query a configured database.

By default, the `IDriver` instance is resolved from the service provider. When no database name is provided, the health check calls `IDriver.VerifyConnectivityAsync`. When a database name is provided, the health check opens a session for that database and executes `RETURN 1`.

```csharp
builder.Services.AddSingleton(_ =>
    GraphDatabase.Driver(
        builder.Configuration["Data:ConnectionStrings:Neo4j"],
        AuthTokens.Basic(
            builder.Configuration["Neo4j:Username"],
            builder.Configuration["Neo4j:Password"])));

builder.Services
    .AddHealthChecks()
    .AddNeo4j();
```

You can configure a custom driver factory and a database name:

```csharp
builder.Services
    .AddHealthChecks()
    .AddNeo4j(
        driverFactory: sp => sp.GetRequiredService<IDriver>(),
        databaseNameFactory: _ => "neo4j");
```

`Neo4jHealthCheck` does not create or cache driver instances. The Neo4j driver is designed to be long-lived, so register `IDriver` as a singleton and dispose it when the application shuts down.
