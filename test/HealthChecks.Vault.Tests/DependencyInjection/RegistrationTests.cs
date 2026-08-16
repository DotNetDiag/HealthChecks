using System.Net;
using HealthChecks.Vault.Tests.Helpers;

namespace HealthChecks.Vault.Tests.DependencyInjection;

public class vault_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_base_uri()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddVault(new Uri("https://vault.example.com"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("vault");
        check.ShouldBeOfType<VaultHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddVault(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("vault");
        check.ShouldBeOfType<VaultHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddVault(optionsFactory: _ => new VaultHealthCheckOptions());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("vault");
        check.ShouldBeOfType<VaultHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddVault(new Uri("https://vault.example.com"), name: "my-vault");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-vault");
        check.ShouldBeOfType<VaultHealthCheck>();
    }

    private static HttpClient _clientFactory(IServiceProvider _)
        => new(new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"initialized":true,"sealed":false,"standby":false}"""))
        {
            BaseAddress = new Uri("https://vault.example.com")
        };
}
