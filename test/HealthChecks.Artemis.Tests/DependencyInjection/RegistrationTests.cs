using Apache.NMS;
using NSubstitute;

namespace HealthChecks.Artemis.Tests.DependencyInjection;

public class artemis_registration_should
{
    private const string BrokerUri = "amqp://localhost:61616";

    [Fact]
    public void add_health_check_when_properly_configured_with_broker_uri()
    {
        var services = new ServiceCollection();
        bool setupCalled = false;

        services.AddHealthChecks()
            .AddArtemis(
                BrokerUri,
                options =>
                {
                    setupCalled = true;
                    options.UserName = "artemis";
                });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("artemis");
        check.ShouldBeOfType<ArtemisHealthCheck>();
        setupCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_factory()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();

        services.AddHealthChecks()
            .AddArtemis(connectionFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("artemis");
        check.ShouldBeOfType<ArtemisHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_connection_factory()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IConnectionFactory>());

        services.AddHealthChecks()
            .AddArtemis();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("artemis");
        check.ShouldBeOfType<ArtemisHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_factories()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();
        bool connectionFactoryCalled = false;
        bool optionsFactoryCalled = false;

        services.AddHealthChecks()
            .AddArtemis(
                _ =>
                {
                    connectionFactoryCalled = true;
                    return connectionFactory;
                },
                _ =>
                {
                    optionsFactoryCalled = true;
                    return new ArtemisHealthCheckOptions();
                });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("artemis");
        check.ShouldBeOfType<ArtemisHealthCheck>();
        connectionFactoryCalled.ShouldBeTrue();
        optionsFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();

        services.AddHealthChecks()
            .AddArtemis(connectionFactory, name: "my-artemis-broker");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-artemis-broker");
        check.ShouldBeOfType<ArtemisHealthCheck>();
    }
}
