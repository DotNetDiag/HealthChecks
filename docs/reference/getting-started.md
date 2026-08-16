---
title: Getting Started
permalink: /reference/getting-started/
---

Use this chapter for the smallest reliable setup for a service that exposes health endpoints.

## Choose the right package

- Use `DotNetDiag.HealthChecks.<Provider>` when you need a dependency-specific check such as SQL Server, Redis, MySQL, Azure Service Bus, or Amazon S3.
- Use `DotNetDiag.HealthChecks.UI` when you want a dashboard that polls one or more health endpoints and stores history.
- Use `DotNetDiag.HealthChecks.Publisher.*` or `DotNetDiag.HealthChecks.Prometheus.Metrics` when you need push or scrape integrations.
- Use the [package catalog]({{ '/reference/package-catalog/' | relative_url }}) to choose a package family, then use the [package references]({{ '/reference/readmes/' | relative_url }}) for provider-specific defaults and overloads.

## Install a package

```bash
dotnet add package DotNetDiag.HealthChecks.SqlServer
dotnet add package DotNetDiag.HealthChecks.Redis
```

Preview and integration packages are published through GitHub Packages:

```xml
<add key="DotNetDiag GitHub Packages" value="https://nuget.pkg.github.com/DotNetDiag/index.json" />
```

## Register checks

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(Configuration["ConnectionStrings:sql"])
    .AddRedis(Configuration["ConnectionStrings:redis"]);
```

Each builder extension also supports operational metadata such as a custom name, tags, a failure status, and a timeout.

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(
        connectionString: Configuration["ConnectionStrings:sql"],
        healthQuery: "SELECT 1;",
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: ["db", "sql", "ready"]);
```

## Separate liveness and readiness

Keep liveness lightweight and reserve readiness for external dependencies.

```csharp
services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddSqlServer(
        connectionString: Configuration["ConnectionStrings:sql"],
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"]);

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});
```

## What to read next

- Continue with the [package catalog]({{ '/reference/package-catalog/' | relative_url }}) if you need the correct dependency package.
- Continue with the [HealthChecks UI manual]({{ '/reference/ui-manual/' | relative_url }}) if you want a dashboard.
- Continue with [publishers and metrics]({{ '/reference/publishers-and-metrics/' | relative_url }}) if you need telemetry export.
- Continue with [deployment and integrations]({{ '/reference/deployment-and-integrations/' | relative_url }}) for Docker, Kubernetes, Azure DevOps, and protected UI setups.
