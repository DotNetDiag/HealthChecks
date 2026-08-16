using System.Net;
using HealthChecks.SonnetDB.Tests.Helpers;

namespace HealthChecks.SonnetDB.Tests.DependencyInjection;

public class sonnetdb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_base_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSonnetDB(new Uri("https://sonnetdb.example.com"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("sonnetdb");
        check.ShouldBeOfType<SonnetDBHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSonnetDB(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("sonnetdb");
        check.ShouldBeOfType<SonnetDBHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddSonnetDB(optionsFactory: _ => new SonnetDBHealthCheckOptions());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("sonnetdb");
        check.ShouldBeOfType<SonnetDBHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSonnetDB(new Uri("https://sonnetdb.example.com"), name: "my-sonnetdb-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-sonnetdb-group");
        check.ShouldBeOfType<SonnetDBHealthCheck>();
    }

    private static HttpClient _clientFactory(IServiceProvider _)
        => new(new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"status":"ok"}"""))
        {
            BaseAddress = new Uri("https://sonnetdb.example.com")
        };
}
