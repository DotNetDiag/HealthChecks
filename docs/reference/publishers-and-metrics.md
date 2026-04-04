---
title: Publishers And Metrics
permalink: /reference/publishers-and-metrics/
---

This chapter replaces the push-results section of the repository README and groups the outbound telemetry options in one place.

## Publisher packages

- `DotNetDiag.HealthChecks.Publisher.ApplicationInsights`
- `DotNetDiag.HealthChecks.Publisher.CloudWatch`
- [DotNetDiag.HealthChecks.Publisher.Datadog]({{ '/reference/readmes/src/HealthChecks-Publisher-Datadog/' | relative_url }})
- `DotNetDiag.HealthChecks.Publisher.Prometheus` - deprecated gateway publisher
- `DotNetDiag.HealthChecks.Publisher.Seq`

Install one or more publisher packages alongside the dependency checks you already registered.

```powershell
Install-Package DotNetDiag.HealthChecks.Publisher.ApplicationInsights
Install-Package DotNetDiag.HealthChecks.Publisher.CloudWatch
Install-Package DotNetDiag.HealthChecks.Publisher.Datadog
Install-Package DotNetDiag.HealthChecks.Publisher.Prometheus
Install-Package DotNetDiag.HealthChecks.Publisher.Seq
```

## Add publishers to the builder

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(connectionString: Configuration["ConnectionStrings:sample"])
    .AddCheck<RandomHealthCheck>("random")
    .AddApplicationInsightsPublisher()
    .AddCloudWatchPublisher()
    .AddDatadogPublisher("myservice.healthchecks")
    .AddPrometheusGatewayPublisher();
```

Use the push model when your monitoring destination expects the application to emit health state changes rather than scrape an endpoint.

## Prometheus exporter

If you need Prometheus to scrape metrics directly, use `DotNetDiag.HealthChecks.Prometheus.Metrics` instead of the gateway package.

```powershell
Install-Package DotNetDiag.HealthChecks.Prometheus.Metrics
```

```csharp
app.UseHealthChecksPrometheusExporter();
app.UseHealthChecksPrometheusExporter("/my-health-metrics");
app.UseHealthChecksPrometheusExporter(
    "/my-health-metrics",
    options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK);
```

## Operational notes

- Use publisher packages for downstream systems that want a pushed status model.
- Use the Prometheus exporter when the monitoring system expects a scrape endpoint.
- If you are already using HealthChecks UI, keep the UI API and your exporter endpoints separate so each consumer can use the response shape it expects.