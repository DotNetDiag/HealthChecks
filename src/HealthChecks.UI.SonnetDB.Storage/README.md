## HealthChecks.UI SonnetDB Storage

This package stores HealthChecks UI data using SonnetDB through the SonnetDB Entity Framework Core provider.

```csharp
services
    .AddHealthChecksUI()
    .AddSonnetDBStorage("Data Source=./healthchecks-ui-data");
```

This storage provider targets `net10.0` because `SonnetDB.EntityFrameworkCore` currently ships a `net10.0` provider assembly.
