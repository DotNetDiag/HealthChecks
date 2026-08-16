## Vault Health Check

This health check verifies a [HashiCorp Vault](https://www.vaultproject.io/) server by calling its `/v1/sys/health` endpoint and evaluating the Vault node state reported in the response.

### Defaults

By default, only an active, initialized, and unsealed Vault node is reported as healthy.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddVault(new Uri("http://localhost:8200"));
}
```

### Accepting standby nodes

Vault standby nodes normally return HTTP 429 from `/v1/sys/health`. If your application can use standby or forwarded Vault requests, add the standby statuses that should be accepted.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddVault(
        new Uri("http://localhost:8200"),
        options =>
        {
            options.HealthyStatuses.Add(VaultHealthStatus.Standby);
            options.HealthyStatuses.Add(VaultHealthStatus.PerformanceStandby);
            options.HealthyStatuses.Add(VaultHealthStatus.DisasterRecoverySecondary);
        });
}
```

### Authentication and request customization

The Vault health endpoint is commonly exposed without a token, but proxies or custom Vault policies may require request headers.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddVault(
        new Uri("http://localhost:8200"),
        options =>
        {
            options.ConfigureRequest = request =>
                request.Headers.Add("X-Vault-Token", "vault-token");
        });
}
```

### Custom endpoint path

Use `HealthEndpointPath` when Vault is routed behind a reverse proxy or when query string options such as `standbyok=true` are required.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.AddVault(
        new Uri("https://gateway.example.com"),
        options => options.HealthEndpointPath = "/vault/v1/sys/health?standbyok=true");
}
```
