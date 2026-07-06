using NSubstitute;
using StackExchange.Redis;

namespace HealthChecks.Valkey.Tests.DependencyInjection;

public class valkey_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddValkey("localhost:6379");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("valkey");
        check.ShouldBeOfType<ValkeyHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddValkey("localhost:6379", name: "my-valkey");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-valkey");
        check.ShouldBeOfType<ValkeyHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddValkey(_ =>
            {
                factoryCalled = true;
                return "localhost:6379";
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("valkey");
        check.ShouldBeOfType<ValkeyHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_with_connection_multiplexer_when_properly_configured()
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        var services = new ServiceCollection();

        services.AddHealthChecks()
            .AddValkey(connectionMultiplexer, name: "my-valkey");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-valkey");
        check.ShouldBeOfType<ValkeyHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_multiplexer_when_properly_configured()
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        var services = new ServiceCollection();

        services.AddSingleton(connectionMultiplexer);
        var factoryCalled = false;

        services.AddHealthChecks()
            .AddValkey(sp =>
            {
                factoryCalled = true;
                return sp.GetRequiredService<IConnectionMultiplexer>();
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("valkey");
        check.ShouldBeOfType<ValkeyHealthCheck>();
        factoryCalled.ShouldBeFalse();
    }
}

