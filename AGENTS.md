# AI Working Rules

All AI agents working in this repository must treat formatting as a blocking CI requirement.

## Hard Requirement

- Do not consider a .NET change finished until the relevant `dotnet format --verify-no-changes --severity warn` command passes.
- Do not leave known formatting or analyzer issues behind.
- Do not bypass failures by weakening `.editorconfig`, changing warning settings, or adding suppressions unless the user explicitly asks for that.

## What CI Enforces

- Shared package workflows run `dotnet format --no-restore --verify-no-changes --severity warn` against the source project and its paired test project.
- UI workflows run the same format check across the full UI project set.
- The repository uses `.editorconfig`, Roslynator analyzers, and `TreatWarningsAsErrors=true`, so style warnings frequently become CI failures.

## Required Validation

- If you changed one source project and one test project, run the same format commands CI uses for those projects.
- If you changed shared files like `test/_SHARED/**`, `build/docker-images/HealthChecks.UI.Image/**`, `Directory.Build.props`, `Directory.Build.targets`, or `Directory.Packages.props`, run a broader validation such as:

```powershell
dotnet format AspNetCore.Diagnostics.HealthChecks.sln --verify-no-changes --severity warn
```

- When practical, also run representative `dotnet build` or `dotnet test` commands for the affected projects after formatting is clean.

## New Health Check Package Standard

- Treat a new health check package as incomplete unless it includes the same supporting surface as comparable packages in this repository.
- Use the closest existing provider family as the baseline for completeness. At minimum, check for source and test projects, solution membership, central package versions, dependency-injection registration, API approval coverage, conformance or equivalent registration tests, behavior tests, package README, root README/package catalog entries, roadmap updates, GitHub workflow coverage, Codecov flags, and labeler entries when comparable packages have them.
- Do not ship a new health check with a thinner public API, fewer registration options, missing docs, missing CI, or weaker validation than adjacent packages unless the user explicitly accepts that scope.
- When adding future roadmap health checks, state any intentionally omitted supporting item and why before considering the task complete.

## Dependency Upgrade Rule

- Before changing NuGet package versions, run the repository dependency checker:

```powershell
.\tools\Get-NuGetEraUpdates.ps1 -OnlyOutdated
```

- Treat target frameworks separately. Packages used by `net8.0` must stay on the latest version appropriate for the .NET 8 era; packages used by `net10.0` can move to the latest compatible .NET 10-era version.
- Do not blindly apply the NuGet-wide latest version to every target framework. This is especially important for runtime/platform packages such as `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, `Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore*`, common EF Core providers, and `System.*`.
- If the checker marks `ConditionReviewNeeded`, inspect the MSBuild condition manually before changing the version.
- Preserve intentional compatibility ranges such as RabbitMQ v6 package ranges unless the user explicitly asks to break that compatibility boundary.

## Completion Rule

Before finishing any task that edits C# or MSBuild files, report which `dotnet format`, build, or test commands were run and whether they passed.
