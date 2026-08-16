# Grafana Health Check

This health check verifies a [Grafana](https://grafana.com/grafana/) instance through the Grafana `/api/health` endpoint.

## Defaults

By default, the Grafana health endpoint is checked and the `database` field returned by Grafana must report `ok`.

```csharp
void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddGrafana(new Uri("https://grafana.example.com"));
}
```

## Custom endpoint or request

If Grafana is exposed through a reverse proxy path, configure `HealthEndpointPath`. For authenticated or proxied setups, use `ConfigureRequest` or configure the named HTTP client.

```csharp
using HealthChecks.Grafana;
using System.Net.Http.Headers;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddGrafana(new Uri("https://grafana.example.com"), options =>
        {
            options.HealthEndpointPath = "/grafana/api/health";
            options.ConfigureRequest = request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");
            };
        });
}
```

## Customization

You can additionally add the following parameters:

- `clientFactory`: A factory method to provide an `HttpClient` instance.
- `optionsFactory`: A factory method to provide a `GrafanaHealthCheckOptions` instance.
- `name`: The health check name. The default is `grafana`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
- `configureClient`: A callback to configure the named Grafana health check `HttpClient`.
- `configurePrimaryHttpMessageHandler`: A callback to configure the named Grafana health check HTTP message handler.
