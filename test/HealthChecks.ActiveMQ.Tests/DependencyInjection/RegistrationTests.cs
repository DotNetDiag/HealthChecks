using Apache.NMS;
using NSubstitute;

namespace HealthChecks.ActiveMQ.Tests.DependencyInjection;

public class activemq_registration_should
{
    private const string BrokerUri = "activemq:tcp://localhost:61616";

    [Fact]
    public void add_health_check_when_properly_configured_with_broker_uri()
    {
        var services = new ServiceCollection();
        bool setupCalled = false;

        services.AddHealthChecks()
            .AddActiveMQ(
                BrokerUri,
                options =>
                {
                    setupCalled = true;
                    options.UserName = "admin";
                });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("activemq");
        check.ShouldBeOfType<ActiveMQHealthCheck>();
        setupCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_factory()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();

        services.AddHealthChecks()
            .AddActiveMQ(connectionFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("activemq");
        check.ShouldBeOfType<ActiveMQHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_connection_factory()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IConnectionFactory>());

        services.AddHealthChecks()
            .AddActiveMQ();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("activemq");
        check.ShouldBeOfType<ActiveMQHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_factories()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();
        bool connectionFactoryCalled = false;
        bool optionsFactoryCalled = false;

        services.AddHealthChecks()
            .AddActiveMQ(
                _ =>
                {
                    connectionFactoryCalled = true;
                    return connectionFactory;
                },
                _ =>
                {
                    optionsFactoryCalled = true;
                    return new ActiveMQHealthCheckOptions();
                });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("activemq");
        check.ShouldBeOfType<ActiveMQHealthCheck>();
        connectionFactoryCalled.ShouldBeTrue();
        optionsFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var connectionFactory = Substitute.For<IConnectionFactory>();

        services.AddHealthChecks()
            .AddActiveMQ(connectionFactory, name: "my-activemq-broker");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-activemq-broker");
        check.ShouldBeOfType<ActiveMQHealthCheck>();
    }
}
