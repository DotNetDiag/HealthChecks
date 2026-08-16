using DotPulsar.Abstractions;
using NSubstitute;

namespace HealthChecks.Apache.Pulsar.Tests.DependencyInjection;

public class pulsar_registration_should
{
    [Fact]
    public void add_broker_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IPulsarClient>());
        services
            .AddHealthChecks()
            .AddPulsar();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("pulsar");
        check.ShouldBeOfType<PulsarHealthCheck>();
    }

    [Fact]
    public void add_broker_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services
            .AddHealthChecks()
            .AddPulsar(_ => Substitute.For<IPulsarClient>());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("pulsar");
        check.ShouldBeOfType<PulsarHealthCheck>();
    }

    [Fact]
    public void add_named_broker_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IPulsarClient>());
        services
            .AddHealthChecks()
            .AddPulsar(name: "my-pulsar");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-pulsar");
        check.ShouldBeOfType<PulsarHealthCheck>();
    }

    [Fact]
    public void add_admin_health_check_when_properly_configured_with_service_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddPulsarAdmin(new Uri("http://localhost:8080"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("pulsar_admin");
        check.ShouldBeOfType<PulsarAdminHealthCheck>();
    }

    [Fact]
    public void add_named_admin_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddPulsarAdmin(new Uri("http://localhost:8080"), name: "my-pulsar-admin");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-pulsar-admin");
        check.ShouldBeOfType<PulsarAdminHealthCheck>();
    }
}
