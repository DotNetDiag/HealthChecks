namespace HealthChecks.IoTDB.Tests.DependencyInjection;

public class iotdb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddIoTDB("DataSource=localhost;Port=6667;Username=root;Password=root");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iotdb");
        check.ShouldBeOfType<IoTDBHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddIoTDB("DataSource=localhost;Port=6667;Username=root;Password=root", name: "my-iotdb-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-iotdb-1");
        check.ShouldBeOfType<IoTDBHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_options_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddIoTDB(new IoTDBHealthCheckOptions
            {
                ConnectionString = "DataSource=localhost;Port=6667;Username=root;Password=root",
                EnableRpcCompression = false
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iotdb");
        check.ShouldBeOfType<IoTDBHealthCheck>();
    }
}
