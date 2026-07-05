using SurrealDb.Net;

namespace HealthChecks.SurrealDb.Tests.DependencyInjection;

public class surrealdb_registration_should
{
    private const string ConnectionString = "Server=http://localhost:8000;Namespace=test;Database=test;Username=root;Password=root";

    [Fact]
    public async Task add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString);
        services
            .AddHealthChecks()
            .AddSurreal();

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("surrealdb");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
    }

    [Fact]
    public async Task add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString);
        services
            .AddHealthChecks()
            .AddSurreal(name: "my-surrealdb-1");

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-surrealdb-1");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
    }

    [Fact]
    public async Task add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSurreal(ConnectionString, ServiceLifetime.Singleton);
        bool factoryCalled = false;
        services.AddHealthChecks()
            .AddSurreal(sp =>
            {
                factoryCalled = true;
                return sp.GetRequiredService<ISurrealDbClient>();
            });

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("surrealdb");
        check.ShouldBeOfType<SurrealDbHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }
}
