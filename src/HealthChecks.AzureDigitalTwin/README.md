# **Digital Twin Health Check**

Azure Digital Twins is an Internet of Things (IoT) platform that enables you to create a digital representation of real-world things, places, business processes, and people.

For more information about Azure Digital Twin please check [Azure Digital Twin Home](https://azure.microsoft.com/en-us/services/digital-twins/)

This health check can check the Digital Twin:

- liveness connection status.
- state of the model definition
- status of an instance

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name.
  <br/>Default for liveness if not specified is `azuredigitaltwin`.
  <br/>Default for model state if not specified is `azuredigitaltwinmodels`.
  <br/>Default for instance status if not specified is `azuredigitaltwininstance`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

---

### How to install

You can download the latest version from nuget packages:

Through Visual Studio:

```
Install-Package DotNetDiag.HealthChecks.AzureDigitalTwin
```

Or through CLI:

```
dotnet add package DotNetDiag.HealthChecks.AzureDigitalTwin
```

---

## _Digital Twin Liveness Health Check_

This health check reads a specified Azure Digital Twins resource through Azure Resource Manager to verify authentication, connectivity, and access to that resource. The credential needs permission to read the resource, for example the Reader role assigned at the resource scope. This check does not verify model or twin instance availability at the data plane endpoint; use the model or instance health check for those checks.

Pass the full Azure resource ID in this format: `/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DigitalTwins/digitalTwinsInstances/{resourceName}`.

### Example Usage

You can add health check with the default client arguments...

```cs
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwin(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId",
            "/subscriptions/my-subscription-id/resourceGroups/my-resource-group/providers/Microsoft.DigitalTwins/digitalTwinsInstances/my-digital-twins");
}
```

... or with an Azure SDK `TokenCredential`, such as `DefaultAzureCredential`:

```cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    TokenCredential credentials = new DefaultAzureCredential();
    services
        .AddHealthChecks()
        .AddAzureDigitalTwin(
            credentials,
            "/subscriptions/my-subscription-id/resourceGroups/my-resource-group/providers/Microsoft.DigitalTwins/digitalTwinsInstances/my-digital-twins");
}
```

### Migration from the legacy management SDK

The liveness check now uses `Azure.ResourceManager.DigitalTwins` instead of `Microsoft.Azure.Management.DigitalTwins` and the Fluent resource management SDK. It reads a specified resource instead of listing the provider's supported operations. Both `AddAzureDigitalTwin` overloads and the `AzureDigitalTwinSubscriptionHealthCheck` constructors now require a `resourceId`. The `clientId`, `clientSecret`, and `tenantId` registration remains available with this additional argument.

This is a breaking API change: `ServiceClientCredentials` overloads and the protected `ServiceClientCredentials` field have been removed. Pass an `Azure.Core.TokenCredential` instead; for service principals, use `new ClientSecretCredential(tenantId, clientId, clientSecret)`. For custom derived health checks, the protected `ManagementClientConnections` dictionary now stores `Azure.ResourceManager.ArmClient` values, and `CreateManagementClient()` returns `ArmClient`.

---

## _Digital Twin Model Health Check_

This health check receives a list of models ids, and check if the Digital Twin has all models match with them.
If the health check detect an `out of sync` models return the data with those elements:

- `unregistered`: those models that exist in model definition but not in the Digital Twin

### Example Usage

<br/>_C# Configuration:_

You can also add health check with the default client arguments...

```cs
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinModels(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId",
            "https://my-awesome-dt-host",
            ["my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1"]);
}
```

... or with the token credentials flow that you want:

```cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public void ConfigureServices(IServiceCollection services)
{
    TokenCredential credentials = new DefaultAzureCredential();
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinModels(
            credentials,
            "https://my-awesome-dt-host",
            ["my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1"],
            failureStatus: HealthStatus.Degraded);
}
```

<small>NOTE: This sample provides a Degraded status if this Health Check fails because it will check for a non sync model state (instead of a real connection status), and the resource is responding at the client call.</small>

<br/>_Failure status response:_

```json
azuredigitaltwinmodels:
{
  data:
  {
    unregistered: [ "my:dt:definition_b;1" ]
  },
  description: "The digital twin is out of sync with the models provided",
  duration: "00:00:17.6056085",
  exception: null,
  status: 1,
  tags: [ "ready" ]
}
```

---

## _Digital Twin Instance Health Check_

This health check returns the status of a given instance.

### Example Usage

<br/>_C# Configuration:_

You can also add health check with the default client arguments...

```cs
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinInstance(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId",
            "https://my-awesome-dt-host",
            "my_dt_instance_name");
}
```

... or with the token credentials flow that you want:

```cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    TokenCredential credentials = new DefaultAzureCredential();
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinInstance(
            credentials,
            "https://my-awesome-dt-host",
            "my_dt_instance_name");
}
```
