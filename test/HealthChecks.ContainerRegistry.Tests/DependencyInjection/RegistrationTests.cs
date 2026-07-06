using System.Net;
using HealthChecks.ContainerRegistry.Tests.Helpers;

namespace HealthChecks.ContainerRegistry.Tests.DependencyInjection;

public class containerregistry_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_base_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddContainerRegistry(new Uri("https://registry.example.com"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("container_registry");
        check.ShouldBeOfType<ContainerRegistryHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddContainerRegistry(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("container_registry");
        check.ShouldBeOfType<ContainerRegistryHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddContainerRegistry(optionsFactory: _ => new ContainerRegistryHealthCheckOptions
            {
                RegistryEndpointPath = "/v2/"
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("container_registry");
        check.ShouldBeOfType<ContainerRegistryHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddContainerRegistry(new Uri("https://registry.example.com"), name: "my-registry-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-registry-group");
        check.ShouldBeOfType<ContainerRegistryHealthCheck>();
    }

    private static HttpClient _clientFactory(IServiceProvider _)
        => new(new RecordingHttpMessageHandler(HttpStatusCode.OK))
        {
            BaseAddress = new Uri("https://registry.example.com")
        };
}
