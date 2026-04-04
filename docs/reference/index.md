---
title: Reference Manual
permalink: /reference/
---

The reference manual collects short implementation examples and the operational cautions that come up most often when integrating the packages in real services.

## Project README catalog

- [Project READMEs]({{ '/reference/readmes/' | relative_url }}) publishes every repository `README.md` outside `docs/` under a generated documentation path.
- Use it when you need package-specific defaults, extension setup notes, or the original per-project usage snippets.

## Basic package registration

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(
        connectionString: Configuration["ConnectionStrings:sql"],
        healthQuery: "SELECT 1;",
        name: "sql",
        tags: new[] { "db", "sql", "ready" })
    .AddRedis(
        redisConnectionString: Configuration["ConnectionStrings:redis"],
        name: "redis",
        tags: new[] { "cache", "ready" });
```

Use focused package registrations instead of installing every package from the catalog. Each check should represent a dependency your service actually relies on.

## Separate liveness and readiness

```csharp
services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddSqlServer(
        connectionString: Configuration["ConnectionStrings:sql"],
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready" });

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});
```

This pattern keeps the process liveness signal lightweight while the readiness endpoint reflects downstream dependencies.

## Add HealthChecks UI

```csharp
services
    .AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(30);
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.AddHealthCheckEndpoint("api", "/health/ready");
    })
    .AddInMemoryStorage();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/healthchecks-ui";
    options.ApiPath = "/healthchecks-api";
});
```

If you need container-specific settings, continue with the [UI Docker image guide]({{ '/ui-docker/' | relative_url }}). If you need alerting payload examples, use the [webhooks guide]({{ '/webhooks/' | relative_url }}).

## Operational notes

- Keep health checks fast and deterministic. Expensive checks can turn a monitoring endpoint into a source of instability.
- Tune `failureStatus`, tags, and polling cadence for each dependency instead of applying the same defaults everywhere.
- Prefer readiness checks for external dependencies and reserve liveness checks for process-level validation.
- When deploying in Kubernetes, combine this page with the [probe guide]({{ '/kubernetes-liveness/' | relative_url }}) and the [operator guide]({{ '/k8s-operator/' | relative_url }}).
- The package naming in this repository follows the `Aspire.<project name>` convention shown in the root README.
