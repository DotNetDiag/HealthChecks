# Temporary Docker Images Roadmap

This file tracks the short-term cleanup for the two product container images:

- `ghcr.io/dotnetdiag/healthchecksui`
- `ghcr.io/dotnetdiag/healthchecksui-k8s-operator`

## Decision

Keep both images. The UI image is the containerized HealthChecks.UI entry point, and the Kubernetes operator image owns the automatic UI deployment and service discovery scenario. New image work targets GitHub Container Registry under the current GitHub organization namespace and follows the ASP.NET Core container default port `8080`.

## P0 - Restore Build And Runtime Viability

- [x] Update the Kubernetes operator Dockerfile off unsupported .NET 6 images.
- [x] Move the UI image, docs, samples, and operator defaults to ASP.NET Core container port 8080.
- [x] Fix local Docker build scripts so they no longer depend on missing `build/dependencies.props`.
- [x] Build both images locally.
- [x] Smoke test the UI container by starting it and checking the UI endpoint.

## P1 - Make Image Builds Reproducible And Efficient

- [x] Upgrade the UI image Node.js build toolchain from Node 18 to the official `node:latest` image at image build time.
- [x] Upgrade the UI image npm toolchain to the latest npm release at image build time.
- [ ] Replace ad hoc npm install behavior with lockfile-based `npm ci`.
- [ ] Reorder Dockerfile copy/restore/build layers to improve cache reuse.
- [ ] Add BuildKit cache mounts for NuGet and npm where practical.
- [ ] Revisit Dockerfile syntax directives after confirming build agents can pull the Dockerfile frontend image.
- [ ] Add OCI image labels for source, revision, version, and license.
- [x] Switch image publishing defaults from Docker Hub to GitHub Container Registry under `ghcr.io/dotnetdiag`.
- [ ] Add multi-architecture build support for `linux/amd64` and `linux/arm64`.
- [ ] Re-run UI image build with `--pull` once MCR access is stable; the current Dockerfile was revalidated with local cached .NET base images after MCR returned EOF for `mcr.microsoft.com/dotnet/aspnet:10.0-noble`.

## P2 - Add Runtime Hardening And Capabilities

- [ ] Add a lightweight container health endpoint such as `/healthz`.
- [ ] Add header-based authentication for the UI push endpoint while keeping query-string auth temporarily compatible.
- [ ] Make the operator default UI image configurable instead of hardcoding a registry image.
- [ ] Add Kubernetes readiness/liveness probes to the operator and generated UI deployment.
- [ ] Add pod/container `securityContext` defaults where compatible.
- [ ] Add or improve CRD status reporting.
- [ ] Tighten CRD validation/defaulting for image, ports, probes, resources, and path settings.

## Validation Checklist

- [x] `dotnet format AspNetCore.Diagnostics.HealthChecks.sln --verify-no-changes --severity warn`
- [x] `dotnet build .\build\docker-images\HealthChecks.UI.Image\HealthChecks.UI.Image.csproj -c Release`
- [x] `dotnet build .\src\HealthChecks.UI.K8s.Operator\HealthChecks.UI.K8s.Operator.csproj -c Release`
- [ ] `dotnet test .\test\HealthChecks.UI.Tests\HealthChecks.UI.Tests.csproj -c Release`
- [x] `docker buildx build --load --no-cache -f build/docker-images/HealthChecks.UI.Image/Dockerfile -t healthchecksui:local .`
- [x] `docker buildx build --pull --load -f src/HealthChecks.UI.K8s.Operator/Dockerfile -t healthchecksui-k8s-operator:local .`
- [x] `docker run --rm -d --name hc-ui -p 5000:8080 healthchecksui:local`
- [x] `curl.exe -f http://localhost:5000/healthchecks-ui`
- [ ] Kind-based operator e2e: load both images, apply CRD/operator, create a `HealthCheck` resource, create a labeled service, and verify the generated UI deployment/service plus push updates.

## Validation Notes

- `docker buildx build --pull --load --no-cache -f build/docker-images/HealthChecks.UI.Image/Dockerfile -t healthchecksui:local .` was attempted after switching to `node:latest`, but Docker Desktop returned EOF while resolving MCR .NET base image metadata. The same Dockerfile passed with local cached base images.
- `dotnet test .\test\HealthChecks.UI.Tests\HealthChecks.UI.Tests.csproj -c Release -f net10.0 --no-restore` is still blocked locally by unavailable external database services for MySQL, SQL Server, and PostgreSQL.
