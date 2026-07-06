## SonnetDB Health Check

This health check verifies that a SonnetDB server responds from its `/healthz` endpoint and reports status `ok`.

### Defaults

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddSonnetDB(new Uri("http://localhost:5080"));
}
```

### Authentication

If SonnetDB does not allow anonymous probes, configure the outgoing request with a bearer token.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddSonnetDB(
        new Uri("http://localhost:5080"),
        options =>
        {
            options.ConfigureRequest = request =>
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", "sonnetdb-token");
        });
}
```

### Copilot readiness

SonnetDB reports Copilot readiness in the `/healthz` payload. When Copilot is a required dependency for your application, the check can fail if Copilot is enabled but not ready.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddSonnetDB(
        new Uri("http://localhost:5080"),
        options => options.RequireCopilotReady = true);
}
```
