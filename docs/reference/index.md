---
title: Reference Manual
permalink: /reference/
---

The reference manual is now organized as themed chapters instead of mirroring the root README as one large page. Read it in order when you are onboarding a service, or jump directly to the area you are actively integrating.

## Core chapters

1. [Getting Started]({{ '/reference/getting-started/' | relative_url }})
2. [Package Catalog]({{ '/reference/package-catalog/' | relative_url }})
3. [Publishers And Metrics]({{ '/reference/publishers-and-metrics/' | relative_url }})
4. [HealthChecks UI Manual]({{ '/reference/ui-manual/' | relative_url }})
5. [Deployment And Integrations]({{ '/reference/deployment-and-integrations/' | relative_url }})
6. [Tutorials And Samples]({{ '/reference/tutorials/' | relative_url }})
7. [Contributing]({{ '/reference/contributing/' | relative_url }})

## Supporting material

- [Project READMEs]({{ '/reference/readmes/' | relative_url }}) contains generated pages for package, extension, and sample README files that live outside `docs/`.
- [Documentation guides]({{ '/guides/' | relative_url }}) contains operational articles for Docker, webhooks, branding, and Kubernetes.

## Scenario paths

### Health Checks Only

1. Start with [Getting Started]({{ '/reference/getting-started/' | relative_url }}) for the minimal registration pattern.
2. Continue to [Package Catalog]({{ '/reference/package-catalog/' | relative_url }}) to choose the provider package you actually need.
3. Use [Project READMEs]({{ '/reference/readmes/' | relative_url }}) when you need provider-specific defaults, overloads, or credential examples.
4. Open [Publishers And Metrics]({{ '/reference/publishers-and-metrics/' | relative_url }}) only if this service also needs outbound telemetry or Prometheus scraping.

### Add The UI

1. Start with [Getting Started]({{ '/reference/getting-started/' | relative_url }}) if the service does not already expose a UI-compatible health endpoint.
2. Move to [HealthChecks UI Manual]({{ '/reference/ui-manual/' | relative_url }}) for dashboard setup, storage, polling, and configuration.
3. Use the operational guides for the next concern: [UI Docker image]({{ '/ui-docker/' | relative_url }}), [Webhooks and failure notifications]({{ '/webhooks/' | relative_url }}), [Interface styling and branding]({{ '/styles-branding/' | relative_url }}).
4. Use [Project READMEs]({{ '/reference/readmes/' | relative_url }}) as appendix material when a storage provider or extension has extra setup details.

### Add Kubernetes

1. Start with [Deployment And Integrations]({{ '/reference/deployment-and-integrations/' | relative_url }}) to see the full deployment surface first.
2. Read [Liveness and readiness probes]({{ '/kubernetes-liveness/' | relative_url }}) if you are only exposing health endpoints to Kubernetes.
3. Read [Kubernetes operator]({{ '/k8s-operator/' | relative_url }}) and [Automatic Kubernetes service discovery]({{ '/k8s-ui-discovery/' | relative_url }}) if the rollout includes HealthChecks UI inside the cluster.
4. Return to [HealthChecks UI Manual]({{ '/reference/ui-manual/' | relative_url }}) when the cluster deployment also needs UI storage, notification, or dashboard configuration details.

## Notes

The generated README catalog remains available as appendix material, but the reference manual is now the primary narrative entry point for this site.
Use the README appendix when you need package-level edge cases, but start from these scenario paths when you want the supported integration path.
