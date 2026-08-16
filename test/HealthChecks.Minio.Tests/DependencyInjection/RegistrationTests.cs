using Minio;

namespace HealthChecks.Minio.Tests.DependencyInjection;

public class minio_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_client_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMinio(_clientFactory);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("minio");
        check.ShouldBeOfType<MinioHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_client()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(_clientFactory)
            .AddHealthChecks()
            .AddMinio(optionsFactory: _ => new MinioHealthCheckOptions
            {
                HealthCheckType = MinioHealthCheckType.Live
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("minio");
        check.ShouldBeOfType<MinioHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMinio(clientFactory: _clientFactory, name: "my-minio-group");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-minio-group");
        check.ShouldBeOfType<MinioHealthCheck>();
    }

    private static IMinioClient _clientFactory(IServiceProvider _)
        => new MinioClient()
            .WithEndpoint("127.0.0.1", 1)
            .WithCredentials("access-key", "secret-key")
            .WithSSL(false)
            .Build();
}
