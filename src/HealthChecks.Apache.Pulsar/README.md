## Apache Pulsar Health Check

This health check verifies [Apache Pulsar](https://pulsar.apache.org/) broker and admin endpoint availability.

The broker check uses the configured `IPulsarClient` and publishes a small health check message to a configured topic. The admin check calls the Pulsar admin health endpoint, `/admin/v2/brokers/health`, and treats successful HTTP responses as healthy.

```csharp
using DotPulsar;
using DotPulsar.Abstractions;

builder.Services.AddSingleton<IPulsarClient>(_ =>
    PulsarClient.Builder()
        .ServiceUrl(new Uri("pulsar://localhost:6650"))
        .Build());

builder.Services
    .AddHealthChecks()
    .AddPulsar()
    .AddPulsarAdmin(new Uri("http://localhost:8080"));
```

You can configure the broker topic and admin endpoint:

```csharp
builder.Services
    .AddHealthChecks()
    .AddPulsar(
        optionsFactory: _ => new PulsarHealthCheckOptions
        {
            Topic = "persistent://public/default/healthchecks"
        })
    .AddPulsarAdmin(
        new Uri("http://localhost:8080"),
        options => options.HealthEndpoint = "/admin/v2/brokers/health");
```

`PulsarHealthCheck` does not create or dispose `IPulsarClient` instances. Register `IPulsarClient` as a singleton so the application owns the client lifetime.
