---
layout: landing
title: Guides
description: Operational guides for Docker, branding, webhook notifications, and Kubernetes rollouts.
permalink: /guides/
show_title: false
hero_eyebrow: DotNetDiag HealthChecks
hero_title: Guides
hero_subtitle: Task-oriented articles for running HealthChecks UI, wiring notifications, and deploying workloads into Kubernetes.
hero_primary_url: /reference/ui-manual/
hero_primary_label: Open UI Manual
hero_secondary_url: /reference/deployment-and-integrations/
hero_secondary_label: Open Deployment Chapter
paths_kicker: Start from the task
paths_title: Common guide paths
paths_description: Use the manual to choose the feature, then switch here for the operational steps.
landing_paths:
  - kicker: Dashboard
    title: Run the UI
    description: Start with storage, runtime configuration, and container deployment for the dashboard.
    url: /ui-docker/
  - kicker: Alerts
    title: Add notifications
    description: Push failures into webhook targets such as Teams or Azure Functions.
    url: /webhooks/
  - kicker: Cluster
    title: Deploy to Kubernetes
    description: Combine probes, discovery, and operator workflows for in-cluster setups.
    url: /k8s-operator/
landing_sections:
  - kicker: UI operations
    title: Run and customize the dashboard
    description: Articles for teams operating the HealthChecks UI in Docker or other hosted environments.
    items:
      - title: UI Docker image
        description: Run the published image and wire configuration through environment variables or container settings.
        url: /ui-docker/
      - title: Interface styling and branding
        description: Change logos, colors, and presentation without maintaining a long-lived UI fork.
        url: /styles-branding/
      - title: Webhooks and failure notifications
        description: Send failures to external systems and tune the operational notification path.
        url: /webhooks/
  - kicker: Kubernetes
    title: Roll out in-cluster
    description: Guidance for probes, service discovery, and operator-managed HealthChecks UI resources.
    items:
      - title: Liveness and readiness probes
        description: Expose endpoints that Kubernetes can use for rollout, restart, and readiness decisions.
        url: /kubernetes-liveness/
      - title: Automatic Kubernetes service discovery
        description: Let the UI discover cluster services instead of manually registering every endpoint.
        url: /k8s-ui-discovery/
      - title: Kubernetes operator
        description: Use the operator workflow when HealthChecks UI becomes part of the platform deployment model.
        url: /k8s-operator/
---

Use these guides when the manual chapter tells you what feature to adopt and you now need the operational setup details.
