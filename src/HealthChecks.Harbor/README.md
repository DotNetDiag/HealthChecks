# Harbor Health Check

This health check verifies a [Harbor](https://goharbor.io/) instance through the Harbor `/api/v2.0/health` endpoint.

## Defaults

By default, the Harbor health endpoint is checked and every component returned by Harbor must report `healthy`.

```csharp
void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddHarbor(new Uri("https://harbor.example.com"));
}
```

## Required components

Use `RequiredComponents` when your deployment must expose specific Harbor services, such as registry, database, job service, or portal.

```csharp
using HealthChecks.Harbor;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddHarbor(new Uri("https://harbor.example.com"), options =>
        {
            options.RequiredComponents.Add("registry");
            options.RequiredComponents.Add("database");
            options.RequiredComponents.Add("jobservice");
            options.RequiredComponents.Add("portal");
        });
}
```

## Custom endpoint or request

If Harbor is exposed through a reverse proxy path, configure `HealthEndpointPath`. For authenticated or proxied setups, use `ConfigureRequest` or configure the named HTTP client.

```csharp
using HealthChecks.Harbor;
using System.Net.Http.Headers;

void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddHarbor(new Uri("https://harbor.example.com"), options =>
        {
            options.HealthEndpointPath = "/harbor/api/v2.0/health";
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
- `optionsFactory`: A factory method to provide a `HarborHealthCheckOptions` instance.
- `name`: The health check name. The default is `harbor`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
- `configureClient`: A callback to configure the named Harbor health check `HttpClient`.
- `configurePrimaryHttpMessageHandler`: A callback to configure the named Harbor health check HTTP message handler.
