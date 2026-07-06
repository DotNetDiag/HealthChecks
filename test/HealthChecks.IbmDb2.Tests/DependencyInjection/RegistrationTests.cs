using IBM.Data.Db2;

namespace HealthChecks.IbmDb2.Tests.DependencyInjection;

public class ibm_db2_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddIbmDb2("connectionstring")
            .Services;

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ibmdb2");
        check.ShouldBeOfType<IbmDb2HealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddIbmDb2("connectionstring", name: "my-ibm-db2")
            .Services;

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-ibm-db2");
        check.ShouldBeOfType<IbmDb2HealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        bool factoryCalled = false;

        services.AddHealthChecks()
            .AddIbmDb2(_ =>
            {
                factoryCalled = true;
                return "connectionstring";
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ibmdb2");
        check.ShouldBeOfType<IbmDb2HealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_with_options_when_properly_configured()
    {
        var healthCheckOptions = new IbmDb2HealthCheckOptions
        {
            ConnectionString = "connectionstring"
        };

        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddIbmDb2(healthCheckOptions)
            .Services;

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ibmdb2");
        check.ShouldBeOfType<IbmDb2HealthCheck>();
    }

    [LinuxFact]
    public async Task invoke_configure_when_defined()
    {
        var services = new ServiceCollection();
        bool invoked = false;
        const string connectionString = "Server=127.0.0.1:1;Database=testdb;UID=db2inst1;PWD=password;Connect Timeout=1;";

        void Configure(DB2Connection connection)
        {
            invoked = true;
            connection.ConnectionString.ShouldBe(connectionString);
        }

        services.AddHealthChecks()
            .AddIbmDb2(connectionString, configure: Configure);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });

        invoked.ShouldBeTrue();
    }
}
