---
layout: landing
title: Reference Manual
description: The primary manual for package selection, UI configuration, deployment, observability, and package reference pages.
permalink: /reference/
show_title: false
hero_eyebrow: DotNetDiag HealthChecks
hero_title: Reference Manual
hero_subtitle: Start from a supported integration path, then open package references when you need provider-specific options, overloads, or implementation notes.
hero_primary_url: /reference/getting-started/
hero_primary_label: Start Here
hero_secondary_url: /reference/readmes/
hero_secondary_label: Open Package References
paths_kicker: Read by scenario
paths_title: Supported integration paths
paths_description: Follow the path that matches the rollout you are doing today, then branch into chapter or appendix detail only when needed.
landing_paths:
  - kicker: Minimal service
    title: Health checks only
    description: Register checks, choose provider packages, and expose health endpoints with the smallest footprint.
    url: /reference/getting-started/
  - kicker: Dashboard rollout
    title: Add the UI
    description: Configure dashboard storage, polling, notifications, branding, and runtime deployment.
    url: /reference/ui-manual/
  - kicker: Cluster rollout
    title: Add Kubernetes
    description: Combine probes, discovery, and operator-based deployment in cluster environments.
    url: /reference/deployment-and-integrations/
landing_sections:
  - kicker: Core manual
    title: Read by topic
    description: These chapters form the main narrative path of the manual.
    items:
      - title: Getting Started
        description: Minimal registration patterns, endpoint exposure, and the first package decision.
        url: /reference/getting-started/
      - title: Package Catalog
        description: Choose the right package family before you drill into provider-specific package reference pages.
        url: /reference/package-catalog/
      - title: Publishers and Metrics
        description: Outbound publishing, Prometheus scraping, and observability-focused integrations.
        url: /reference/publishers-and-metrics/
      - title: HealthChecks UI Manual
        description: Dashboard configuration, storage providers, polling cadence, and UI runtime behavior.
        url: /reference/ui-manual/
      - title: Deployment and Integrations
        description: Kubernetes, Azure DevOps, Docker-adjacent deployment, and operational integration notes.
        url: /reference/deployment-and-integrations/
      - title: Tutorials and Samples
        description: Walkthroughs and sample-oriented guidance for teams validating an adoption path.
        url: /reference/tutorials/
  - kicker: Package references
    title: Package references and supporting material
    description: The generated package reference section keeps package, extension, and sample documentation inside the manual without creating a second top-level area.
    items:
      - title: Package References
        description: Browse generated package, extension, and sample reference pages under stable docs URLs.
        url: /reference/readmes/
      - title: Guides
        description: Operational articles for Docker, branding, webhooks, probes, discovery, and the operator.
        url: /guides/
      - title: Contributing
        description: Contribution notes, repository expectations, and workflow guidance for changes.
        url: /reference/contributing/
---

The generated package reference section remains part of the manual, but it now sits behind the main narrative instead of looking like a second top-level documentation area.
