using System.Net;
using HealthChecks.Harbor.Tests.Helpers;

namespace HealthChecks.Harbor.Tests.DependencyInjection;

public class harbor_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_base_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHarbor(new Uri("https://harbor.example.com"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("harbor");
        check.ShouldBeOfType<HarborHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHarbor(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("harbor");
        check.ShouldBeOfType<HarborHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddHarbor(optionsFactory: _ => new HarborHealthCheckOptions
            {
                HealthEndpointPath = "/api/v2.0/health"
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("harbor");
        check.ShouldBeOfType<HarborHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHarbor(new Uri("https://harbor.example.com"), name: "my-harbor-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-harbor-group");
        check.ShouldBeOfType<HarborHealthCheck>();
    }

    private static HttpClient _clientFactory(IServiceProvider _)
        => new(new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"status":"healthy"}"""))
        {
            BaseAddress = new Uri("https://harbor.example.com")
        };
}
