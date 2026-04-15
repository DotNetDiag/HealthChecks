## NTP Server Health Check

This health check verifies that the local system clock is synchronized with an [NTP](https://en.wikipedia.org/wiki/Network_Time_Protocol) server. It queries the server via UDP and compares the returned time to the local clock. The result is `Healthy`, `Degraded`, or `Unhealthy` based on a configurable tolerance.

## NuGet

[![Nuget](https://img.shields.io/nuget/v/DotNetDiag.HealthChecks.NTPServer)](https://www.nuget.org/packages/DotNetDiag.HealthChecks.NTPServer)

```shell
dotnet add package DotNetDiag.HealthChecks.NTPServer
```

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `ntpserver`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Use default settings (`pool.ntp.org`, 10-second tolerance)

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddNTPServer();
}
```

### Specify a custom NTP server and tolerance

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddNTPServer(options =>
        {
            options.NtpServer = "time.windows.com";
            options.ToleranceSeconds = 5.0;
        });
}
```

### Specify a custom NTP server port

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services
        .AddHealthChecks()
        .AddNTPServer(options =>
        {
            options.NtpServer = "time.internal.example";
            options.NtpPort = 10123;
        });
}
```

### Health status thresholds

| Condition | Returned status |
|---|---|
| `\|offset\|` ≤ `ToleranceSeconds` | `Healthy` |
| `ToleranceSeconds` < `\|offset\|` ≤ `ToleranceSeconds × 2` | `Degraded` |
| `\|offset\|` > `ToleranceSeconds × 2` | `FailureStatus` (default `Unhealthy`) |
