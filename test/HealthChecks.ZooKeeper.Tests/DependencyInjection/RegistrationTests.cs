namespace HealthChecks.ZooKeeper.Tests.DependencyInjection;

public class zookeeper_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddZooKeeper("localhost:2181");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("zookeeper");
        check.ShouldBeOfType<ZooKeeperHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddZooKeeper(_ => "localhost:2181");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("zookeeper");
        check.ShouldBeOfType<ZooKeeperHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_connection_string()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton("localhost:2181")
            .AddHealthChecks()
            .AddZooKeeper(optionsFactory: _ => new ZooKeeperHealthCheckOptions());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("zookeeper");
        check.ShouldBeOfType<ZooKeeperHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddZooKeeper("localhost:2181", name: "my-zookeeper-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-zookeeper-group");
        check.ShouldBeOfType<ZooKeeperHealthCheck>();
    }
}
