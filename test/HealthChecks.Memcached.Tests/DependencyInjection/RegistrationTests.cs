using Enyim.Caching;
using NSubstitute;

namespace HealthChecks.Memcached.Tests.DependencyInjection;

public class memcached_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_server()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMemcached("localhost");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("memcached");
        check.ShouldBeOfType<MemcachedHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_options()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMemcached(options =>
            {
                options.AddServer("localhost");
                options.KeyPrefix = "custom_healthcheck_";
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("memcached");
        check.ShouldBeOfType<MemcachedHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMemcached("localhost", name: "my-memcached-cluster");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-memcached-cluster");
        check.ShouldBeOfType<MemcachedHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client()
    {
        IMemcachedClient memcachedClient = Substitute.For<IMemcachedClient>();
        var services = new ServiceCollection();

        services.AddHealthChecks()
            .AddMemcached(memcachedClient);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("memcached");
        check.ShouldBeOfType<MemcachedHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        IMemcachedClient memcachedClient = Substitute.For<IMemcachedClient>();
        var services = new ServiceCollection();
        var factoryCalled = false;

        services.AddHealthChecks()
            .AddMemcached(_ =>
            {
                factoryCalled = true;
                return memcachedClient;
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("memcached");
        check.ShouldBeOfType<MemcachedHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }
}
