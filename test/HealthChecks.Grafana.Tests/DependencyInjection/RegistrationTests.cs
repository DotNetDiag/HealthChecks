using System.Net;
using HealthChecks.Grafana.Tests.Helpers;

namespace HealthChecks.Grafana.Tests.DependencyInjection;

public class grafana_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_base_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddGrafana(new Uri("https://grafana.example.com"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("grafana");
        check.ShouldBeOfType<GrafanaHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddGrafana(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("grafana");
        check.ShouldBeOfType<GrafanaHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddGrafana(optionsFactory: _ => new GrafanaHealthCheckOptions
            {
                HealthEndpointPath = "/api/health"
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("grafana");
        check.ShouldBeOfType<GrafanaHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddGrafana(new Uri("https://grafana.example.com"), name: "my-grafana-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-grafana-group");
        check.ShouldBeOfType<GrafanaHealthCheck>();
    }

    private static HttpClient _clientFactory(IServiceProvider _)
        => new(new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"database":"ok"}"""))
        {
            BaseAddress = new Uri("https://grafana.example.com")
        };
}
