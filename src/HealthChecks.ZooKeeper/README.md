## ZooKeeper Health Check

This health check verifies that an Apache ZooKeeper ensemble can be reached and that the configured znode exists.

### Defaults

By default, the package checks the root znode `/` with a 10 second session timeout.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddZooKeeper("localhost:2181");
}
```

### Custom znode

Configure `Path` when your application depends on a specific znode being present.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddZooKeeper(
        "zk1:2181,zk2:2181,zk3:2181",
        options => options.Path = "/services/catalog");
}
```

### Session timeout

Use a shorter timeout when health checks must fail quickly.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddZooKeeper(
        "localhost:2181",
        options => options.SessionTimeout = TimeSpan.FromSeconds(3));
}
```
