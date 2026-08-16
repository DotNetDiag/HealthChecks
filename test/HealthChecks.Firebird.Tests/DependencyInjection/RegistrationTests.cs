namespace HealthChecks.Firebird.Tests.DependencyInjection;

public class firebird_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddFirebird("connectionstring");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("firebird");
        check.ShouldBeOfType<FirebirdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddFirebird(_ => "connectionstring");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("firebird");
        check.ShouldBeOfType<FirebirdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_options()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddFirebird(new FirebirdHealthCheckOptions("connectionstring"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("firebird");
        check.ShouldBeOfType<FirebirdHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_options_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddFirebird(_ => new FirebirdHealthCheckOptions("connectionstring"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("firebird");
        check.ShouldBeOfType<FirebirdHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddFirebird("connectionstring", name: "my-firebird-db");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-firebird-db");
        check.ShouldBeOfType<FirebirdHealthCheck>();
    }
}
