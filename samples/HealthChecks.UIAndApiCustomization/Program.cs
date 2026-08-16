using HealthChecks.UI.Client;
using HealthChecks.UIAndApi.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RemoteOptions>(options => builder.Configuration.Bind(options));

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage()
    .Services
    .AddHealthChecks()
    .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-1")
    .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-2")
    .AddUrlGroup(
        sp =>
        {
            var remoteOptions = sp.GetRequiredService<IOptions<RemoteOptions>>().Value;
            return remoteOptions.RemoteDependency;
        },
        "uri-3")
    .AddUrlGroup(new Uri("http://httpbin.org/status/500"), name: "uri-4")
    .Services
    .AddControllers();

var app = builder.Build();

app.UseRouting()
   .UseEndpoints(config =>
   {
       config.MapHealthChecks("/healthz", new HealthCheckOptions
       {
           Predicate = _ => true,
           ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
       });

       config.MapHealthChecksUI(setup =>
       {
           setup.UIPath = "/show-health-ui";
           setup.ApiPath = "/health-ui-api";
           setup.PageTitle = "My wonderful Health Checks UI";
       });

       config.MapDefaultControllerRoute();
   });

app.Run();
