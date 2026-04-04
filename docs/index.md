---
layout: default
title: DotNetDiag HealthChecks
description: Health checks for services, dashboards, and Kubernetes workloads.
show_title: false
---

{% include hero.html title="DotNetDiag HealthChecks" subtitle="Health checks for services, dashboards, and Kubernetes workloads." primary_url="/reference/" primary_label="Open Manual" secondary_url="/guides/" secondary_label="Browse Guides" %}

> Maintained by {{ site.data.authors.primary.name }} / {{ site.data.authors.primary.role }}

DotNetDiag HealthChecks is the actively maintained fork of AspNetCore.Diagnostics.HealthChecks. This site now uses a local template adapted from the JekyllNet sample-site so the docs experience is predictable in GitHub Pages style builds and in local preview.

## Start here

1. [Health checks only]({{ '/reference/' | relative_url }}): Choose provider packages, register checks, and expose liveness and readiness endpoints.
2. [Add the UI]({{ '/reference/ui-manual/' | relative_url }}): Configure the dashboard, storage providers, polling cadence, and failure notifications.
3. [Add Kubernetes]({{ '/reference/deployment-and-integrations/' | relative_url }}): Deploy the UI, enable operator workflows, and integrate probes and discovery in-cluster.

## Site sections

- [Documentation]({{ '/guides/' | relative_url }}) for Docker, UI, webhook, and Kubernetes guides.
- [使用手册]({{ '/reference/' | relative_url }}) for setup, package selection, UI, publishers, deployment chapters, and the generated README appendix.
- [News]({{ '/news/' | relative_url }}) for documentation updates and the historical UI changelog.

## Quick install

```bash
dotnet add package DotNetDiag.HealthChecks.SqlServer
```

## Coverage

- 50+ health check packages for infrastructure and platform dependencies.
- HealthChecks UI guidance for dashboard deployment, branding, notifications, and configuration.
- Kubernetes articles for probes, operator deployment, and service discovery.
- Themed reference chapters plus generated per-project README pages for detailed package notes.
