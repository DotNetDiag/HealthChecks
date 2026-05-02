using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRouting()
    .AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 200, tags: ["memory"])
    .AddCheck(name: "random", () => DateTime.UtcNow.Second % 2 == 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy())
    .Services
    .AddHealthChecksUI(setup =>
    {
        setup.SetHeaderText("Storage providers demo");
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.AddHealthCheckEndpoint("Endpoint2", "/random-health");
    })
    //Uncomment the options below to use different database providers
    //.AddSqlServerStorage("server=localhost;initial catalog=healthchecksui;user id=sa;password=Password12!")
    //.AddSqliteStorage("Data Source = healthchecks.db")
    .AddInMemoryStorage();
//.AddPostgreSqlStorage("Host=localhost;Username=postgres;Password=Password12!;Database=healthchecksui")
//.AddMySqlStorage("Host=localhost;User Id=root;Password=Password12!;Database=UI")

var app = builder.Build();

app.MapHealthChecks("/memory-health", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("memory"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/random-health", new HealthCheckOptions
{
    Predicate = r => r.Name.Equals("random", StringComparison.InvariantCultureIgnoreCase),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI();

app.Run();
