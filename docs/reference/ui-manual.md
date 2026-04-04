---
title: HealthChecks UI Manual
permalink: /reference/ui-manual/
---

Use this chapter for stable dashboard setup, storage, polling, and configuration guidance.

## UI package family

- `DotNetDiag.HealthChecks.UI` - ASP.NET Core dashboard and middleware
- `DotNetDiag.HealthChecks.UI.Client` - response writer and shared abstractions
- `DotNetDiag.HealthChecks.UI.Core` - UI core services
- `DotNetDiag.HealthChecks.UI.Data` - UI data models and database context
- `DotNetDiag.HealthChecks.UI.InMemory.Storage`
- `DotNetDiag.HealthChecks.UI.SqlServer.Storage`
- `DotNetDiag.HealthChecks.UI.SQLite.Storage`
- `DotNetDiag.HealthChecks.UI.PostgreSQL.Storage`
- `DotNetDiag.HealthChecks.UI.MySql.Storage`

## Minimal setup

```csharp
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using HealthChecks.UI.InMemory.Storage;

services
    .AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(30);
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.AddHealthCheckEndpoint("api", "/healthz");
    })
    .AddInMemoryStorage();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    endpoints.MapHealthChecksUI();
});
```

The dashboard is served from `/healthchecks-ui`, while the SPA polls the UI backend API served from `/healthchecks-api` by default.

## Polling, concurrency, and history

```csharp
services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(5);
    setup.SetApiMaxActiveRequests(1);
    setup.MaximumHistoryEntriesPerEndpoint(50);
});
```

- Use `SetEvaluationTimeInSeconds` to control polling cadence.
- Use `SetApiMaxActiveRequests` if your UI backend should reject excess concurrent requests with HTTP 429.
- Use `MaximumHistoryEntriesPerEndpoint` to limit how much timeline data the UI API returns.

## Storage providers

```csharp
services.AddHealthChecksUI().AddInMemoryStorage();
services.AddHealthChecksUI().AddSqlServerStorage("connectionString");
services.AddHealthChecksUI().AddPostgreSqlStorage("connectionString");
services.AddHealthChecksUI().AddMySqlStorage("connectionString");
services.AddHealthChecksUI().AddSqliteStorage("Data Source=sqlite.db");
```

Choose the storage provider based on where you want UI execution history and endpoint metadata to persist.

## Database migrations

Database migrations are enabled by default. Disable them only when your deployment process manages the schema separately.

```csharp
services
    .AddHealthChecksUI(setup => setup.DisableDatabaseMigrations())
    .AddInMemoryStorage();
```

```json
{
  "HealthChecksUI": {
    "DisableMigrations": true
  }
}
```

## Configuration by appsettings or code

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/healthz"
      }
    ],
    "Webhooks": [
      {
        "Name": "teams",
        "Uri": "https://example.test/webhook",
        "Payload": "{...}",
        "RestoredPayload": "{...}"
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

```csharp
services
    .AddHealthChecksUI(setup =>
    {
        setup.AddHealthCheckEndpoint("endpoint1", "http://localhost:8001/healthz");
        setup.AddHealthCheckEndpoint("endpoint2", "http://remoteendpoint:9000/healthz");
        setup.AddWebhookNotification("webhook1", uri: "http://httpbin.org/status/200?code=ax3rt56s", payload: "{...}");
    })
    .AddSqlServerStorage("connectionString");
```

The supported configuration sources are the same ones you already use elsewhere in ASP.NET Core: JSON files, environment variables, user secrets, and code.

## Relative URLs and notifications

When the UI runs in the same process as the health endpoints and webhook receivers, relative paths are supported.

```csharp
services
    .AddHealthChecksUI(setup =>
    {
        setup.AddHealthCheckEndpoint("endpoint1", "/health-databases");
        setup.AddHealthCheckEndpoint("endpoint2", "health-messagebrokers");
        setup.AddWebhookNotification("webhook1", uri: "/notify", payload: "{...}");
        setup.SetNotifyUnHealthyOneTimeUntilChange();
    })
    .AddInMemoryStorage();
```

Use `SetNotifyUnHealthyOneTimeUntilChange` to avoid notification spam while a dependency remains down.

## Styling and HTTP pipeline customization

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecksUI(setup =>
    {
        setup.AddCustomStylesheet("dotnet.css");
    });
});
```

```csharp
services.AddHealthChecksUI(setup =>
{
    setup.ConfigureApiEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "supertoken");
    });
    setup.UseApiEndpointHttpMessageHandler(sp => new HttpClientHandler());
    setup.ConfigureWebhooksEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sampletoken");
    });
    setup.UseWebhookEndpointHttpMessageHandler(sp => new HttpClientHandler());
})
.AddInMemoryStorage();
```

Use these hooks when the dashboard or webhook pipeline must go through a proxy, attach headers, or use custom delegating handlers.

## Related guides

- [UI Docker image]({{ '/ui-docker/' | relative_url }})
- [Webhooks and failure notifications]({{ '/webhooks/' | relative_url }})
- [Interface styling and branding]({{ '/styles-branding/' | relative_url }})
- [UI changelog]({{ '/ui-changelog/' | relative_url }})
