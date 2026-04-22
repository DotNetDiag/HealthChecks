---
layout: landing
title: DotNetDiag HealthChecks
description: Health checks for .NET services, covering 50+ providers, a dashboard UI, and Kubernetes-ready probes.
permalink: /
show_title: false
hero_eyebrow: DotNetDiag HealthChecks
hero_title: Health checks for every .NET service
hero_subtitle: Liveness and readiness probes for 50+ databases, queues, cloud services, and HTTP dependencies, with an optional dashboard UI and a Kubernetes operator.
hero_primary_url: /reference/getting-started/
hero_primary_label: Get Started
hero_secondary_url: /reference/package-catalog/
hero_secondary_label: Browse Packages
paths_kicker: Pick your path
paths_title: Three ways to roll out
paths_description: Most teams start with the minimal service and layer on the UI or Kubernetes pieces as the rollout grows.
landing_paths:
  - kicker: Minimal service
    title: Health checks only
    description: Register checks, choose provider packages, and expose /health endpoints in a few lines.
    url: /reference/getting-started/
  - kicker: Dashboard rollout
    title: Add the UI
    description: Stand up the HealthChecks UI for storage, polling, branding, and failure notifications.
    url: /reference/ui-manual/
  - kicker: Cluster rollout
    title: Add Kubernetes
    description: Probes, service discovery, and the operator for cluster-managed deployments.
    url: /reference/deployment-and-integrations/
landing_sections:
  - kicker: Popular checks
    title: Common integrations
    description: Hand-picked starting points. The full catalog has 50+ packages.
    items:
      - title: SQL Server
        description: Probe SQL Server availability and query responsiveness.
        url: /reference/package-catalog/
      - title: PostgreSQL
        description: Check PostgreSQL connectivity through Npgsql.
        url: /reference/package-catalog/
      - title: Redis
        description: Verify Redis responsiveness for cached and session data.
        url: /reference/package-catalog/
      - title: RabbitMQ
        description: Confirm broker reachability before processing messages.
        url: /reference/package-catalog/
      - title: HTTP endpoints
        description: Probe upstream URIs and external HTTP dependencies.
        url: /reference/package-catalog/
      - title: Kubernetes
        description: Surface cluster service state for liveness and readiness probes.
        url: /reference/package-catalog/
  - kicker: Keep going
    title: Deeper references
    items:
      - title: Reference Manual
        description: Setup, package selection, UI, publishers, deployment, and the generated package index.
        url: /reference/
      - title: Guides
        description: Task-oriented articles for Docker, branding, webhooks, probes, and the operator.
        url: /guides/
      - title: News
        description: Documentation updates and the historical UI changelog.
        url: /news/
      - title: About the fork
        description: Why this fork exists, what it continues from Xabaril, and how to contribute.
        url: /about/
---

> **A community-maintained continuation of [Xabaril/AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks).** That project, shepherded by [Xabaril](https://github.com/Xabaril) and its many contributors, shaped the modern .NET health-check ecosystem. With upstream maintenance paused, DotNetDiag carries the work forward, with deep thanks to every original author whose code and APIs this fork continues.

## 60-second quickstart

Install a provider package:

```bash
dotnet add package DotNetDiag.HealthChecks.SqlServer
```

Register the check and map the endpoint in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default"));

app.MapHealthChecks("/health");
```

That's it. `GET /health` now returns `Healthy` or `Unhealthy`. Add the UI, more providers, or Kubernetes probes from the paths below.
