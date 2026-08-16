namespace HealthChecks.NTPServer.Tests.DependencyInjection;

public class ntpserver_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNTPServer();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ntpserver");
        check.ShouldBeOfType<NTPServerHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNTPServer(name: "my-ntp-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-ntp-1");
        check.ShouldBeOfType<NTPServerHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_options_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNTPServer(options =>
            {
                options.NtpServer = "pool.ntp.org";
                options.ToleranceSeconds = 5.0;
            });

        using var serviceProvider = services.BuildServiceProvider();
        var optionsService = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = optionsService.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("ntpserver");
        check.ShouldBeOfType<NTPServerHealthCheck>();
    }
}
