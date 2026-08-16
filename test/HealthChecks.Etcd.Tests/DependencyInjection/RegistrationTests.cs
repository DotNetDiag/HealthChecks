using dotnet_etcd;

namespace HealthChecks.Etcd.Tests.DependencyInjection;

public class etcd_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddEtcd("http://localhost:2379");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("etcd");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddEtcd(_ => "http://localhost:2379");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("etcd");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_connection_string()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton("http://localhost:2379")
            .AddHealthChecks()
            .AddEtcd(optionsFactory: _ => new EtcdHealthCheckOptions());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("etcd");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client()
    {
        using var client = new EtcdClient("http://localhost:2379");

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddEtcd(client);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("etcd");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        using var client = new EtcdClient("http://localhost:2379");

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddEtcd(_ => client);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("etcd");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddEtcd("http://localhost:2379", name: "my-etcd-cluster");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-etcd-cluster");
        check.ShouldBeOfType<EtcdHealthCheck>();
    }
}
