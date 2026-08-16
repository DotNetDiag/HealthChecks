# Copilot Instructions

This repository treats `dotnet format` as a hard CI gate.

- Before finishing any change to `.cs`, `.csproj`, `.props`, or `.targets` files, run the relevant `dotnet format --verify-no-changes --severity warn` command and make sure it passes.
- Match the workflow behavior when possible:
  - normal package workflows: format the changed source project and its paired test project
  - shared or cross-cutting changes: run `dotnet format AspNetCore.Diagnostics.HealthChecks.sln --verify-no-changes --severity warn`
- If you touch shared files such as `test/_SHARED/**`, `build/docker-images/HealthChecks.UI.Image/**`, `Directory.Build.props`, `Directory.Build.targets`, or `Directory.Packages.props`, assume multiple workflows are affected and validate more broadly.
- Do not silence format or analyzer failures by changing warning settings, weakening `.editorconfig`, or adding suppressions unless the user explicitly requests that tradeoff.
- Prefer minimal fixes that make the formatter and CI pass cleanly.