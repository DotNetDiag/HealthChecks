---
title: Deployment And Integrations
permalink: /reference/deployment-and-integrations/
---

Use this chapter for container, Kubernetes, Azure DevOps, and protected UI integration paths.

## Container images

The published images still use the `xabarilcoding/*` namespace.

- UI image: `xabarilcoding/healthchecksui`
- Kubernetes operator image: `xabarilcoding/healthchecksui-k8s-operator`

Use the [UI Docker image guide]({{ '/ui-docker/' | relative_url }}) when you are deploying the dashboard in a containerized environment.

## Kubernetes

- [Kubernetes operator]({{ '/k8s-operator/' | relative_url }}) for rapid bootstrap of HealthChecks UI workloads.
- [Automatic Kubernetes service discovery]({{ '/k8s-ui-discovery/' | relative_url }}) when you want the UI to discover services instead of registering endpoints manually.
- [Liveness and readiness probes]({{ '/kubernetes-liveness/' | relative_url }}) for application-level health endpoints used by Kubernetes.

## Azure DevOps release gates

Health checks can be used as Azure DevOps release gates through the marketplace extension documented in [the extension reference page]({{ '/reference/readmes/extensions/' | relative_url }}).

Use this integration when deployment promotion must wait for an application-specific health signal instead of relying only on generic availability checks.

## Protected UI with OpenID Connect

If the dashboard should only be visible to authenticated users, protect the UI with standard ASP.NET Core authentication and authorization.

- Sample application: [HealthChecks.UI.Oidc](https://github.com/DotNetDiag/HealthChecks/tree/master/samples/HealthChecks.UI.Oidc)
- Common identity providers: Azure AD, Auth0, Okta, IdentityServer-compatible providers

The dashboard itself does not require a special authentication package. Treat it like any other ASP.NET Core application and apply your normal authentication and authorization pipeline before mapping the UI endpoints.

## Recommended deployment path

1. Start with the [UI manual]({{ '/reference/ui-manual/' | relative_url }}) for endpoint shape, storage, and configuration.
2. Use the [Docker guide]({{ '/ui-docker/' | relative_url }}) if the UI runs in a container.
3. Add the Kubernetes guides when the target environment needs discovery, operator management, or probe integration.
4. Add Azure DevOps release gates only after the UI or JSON health endpoint is already stable and trusted.
