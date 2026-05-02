using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks()
    //.AddRabbitMQ(rabbitConnectionString: "amqp://localhost:5672", name: "rabbit1")
    //.AddRabbitMQ(rabbitConnectionString: "amqp://localhost:6672", name: "rabbit2")
    //.AddSqlServer(connectionString: builder.Configuration["Data:ConnectionStrings:Sample"])
    .AddCheck<RandomHealthCheck>("random")
    //.AddOpenIdConnectServer(new Uri("http://localhost:6060"))
    //.AddAzureServiceBusQueue("...", "que1")
    //.AddAzureServiceBusTopic("...", "to1")
    .AddApplicationInsightsPublisher(saveDetailedReport: true);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
   {
       Predicate = _ => true,
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
   })
   .UseHealthChecksPrometheusExporter("/metrics")
   .UseRouting()
   .UseEndpoints(config => config.MapDefaultControllerRoute());

app.Run();

internal class RandomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow.Minute % 2 == 0)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed", exception: new InvalidCastException("Invalid cast from to to to")));
    }
}
