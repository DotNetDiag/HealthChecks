# Container Registry Health Check

This health check verifies an OCI or Docker Registry HTTP API v2 endpoint through the registry `/v2/` endpoint.

By default, the check reports healthy when the registry endpoint returns a successful HTTP status code. It also reports healthy when a private registry returns `401 Unauthorized` with a `WWW-Authenticate` challenge, because that response shows the registry endpoint is reachable and the authentication challenge is available.

## Basic usage

```csharp
services
    .AddHealthChecks()
    .AddContainerRegistry(new Uri("https://registry.example.com"));
```

## Authenticated registry

Use `ConfigureRequest` or configure the named HTTP client when the health check should send credentials and require a successful response from the registry.

```csharp
using System.Net.Http.Headers;
using HealthChecks.ContainerRegistry;

services
    .AddHealthChecks()
    .AddContainerRegistry(new Uri("https://registry.example.com"), options =>
    {
        options.AllowUnauthorizedResponse = false;
        options.ConfigureRequest = request =>
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        };
    });
```

## Reverse proxy path

If the registry is exposed through a reverse proxy path, configure `RegistryEndpointPath`.

```csharp
using HealthChecks.ContainerRegistry;

services
    .AddHealthChecks()
    .AddContainerRegistry(new Uri("https://proxy.example.com"), options =>
    {
        options.RegistryEndpointPath = "/registry/v2/";
    });
```

## Authentication challenge policy

`AllowUnauthorizedResponse` defaults to `true`, so private registries can be checked without credentials. `RequireAuthenticationChallenge` defaults to `true`, so a `401 Unauthorized` response is only healthy when it includes a `WWW-Authenticate` header.

```csharp
services
    .AddHealthChecks()
    .AddContainerRegistry(new Uri("https://registry.example.com"), options =>
    {
        options.AllowUnauthorizedResponse = true;
        options.RequireAuthenticationChallenge = true;
    });
```

## Parameters

- `baseUri`: The container registry base URI.
- `setup`: A callback to configure `ContainerRegistryHealthCheckOptions`.
- `clientFactory`: An optional factory method to provide the `HttpClient` instance.
- `optionsFactory`: A factory method to provide a `ContainerRegistryHealthCheckOptions` instance.
- `name`: The health check name. The default is `container_registry`.
- `failureStatus`: The status to report when the health check fails. The default is `Unhealthy`.
- `tags`: A list of tags that can be used to filter health checks.
- `timeout`: An optional `TimeSpan` representing the timeout of the check.
- `configureClient`: A callback to configure the named container registry health check `HttpClient`.
- `configurePrimaryHttpMessageHandler`: A callback to configure the named container registry health check HTTP message handler.
