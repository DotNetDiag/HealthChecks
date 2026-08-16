# ActiveMQ Health Check

This health check verifies the ability to connect to an Apache ActiveMQ Classic broker using Apache.NMS.ActiveMQ.

## Example Usage

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddActiveMQ("activemq:tcp://localhost:61616");
}
```

For authenticated brokers:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddActiveMQ(
            "activemq:tcp://localhost:61616",
            options =>
            {
                options.UserName = "admin";
                options.Password = "admin";
                options.RequestTimeout = TimeSpan.FromSeconds(5);
            });
}
```

If the application already owns the `IConnectionFactory`, register it and let the health check resolve it:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddSingleton<IConnectionFactory>(new Apache.NMS.ActiveMQ.ConnectionFactory("activemq:tcp://localhost:61616"))
        .AddHealthChecks()
        .AddActiveMQ();
}
```

Like all `IHealthChecksBuilder` extensions, the following parameters may be overridden:

- `name`: The health check name. Default is `activemq`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

[<<](../../README.md)
