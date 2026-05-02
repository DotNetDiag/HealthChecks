using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage()
    .Services
    .AddHealthChecks()
    .AddCheck<RandomHealthCheck>("random")
    .AddUrlGroup(new Uri("http://httpbin.org/status/200"))
    .Services
    .AddControllers();

var app = builder.Build();

app.UseRouting()
   .UseEndpoints(config =>
   {
       config.MapHealthChecks("healthz", new HealthCheckOptions
       {
           Predicate = _ => true,
           ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
       });
       config.MapHealthChecksUI();
       config.MapDefaultControllerRoute();
   });

app.Run();

internal class RandomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow.Minute % 2 == 0)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed"));
    }
}
