using Neo4j.Driver;
using NSubstitute;

namespace HealthChecks.Neo4j.Tests.DependencyInjection;

public class neo4j_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IDriver>());
        services
            .AddHealthChecks()
            .AddNeo4j();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("neo4j");
        check.ShouldBeOfType<Neo4jHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IDriver>());
        services
            .AddHealthChecks()
            .AddNeo4j(name: "my-neo4j");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-neo4j");
        check.ShouldBeOfType<Neo4jHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_driver_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var driver = Substitute.For<IDriver>();
        bool driverFactoryCalled = false;
        bool databaseNameFactoryCalled = false;

        services
            .AddHealthChecks()
            .AddNeo4j(
                _ =>
                {
                    driverFactoryCalled = true;
                    return driver;
                },
                _ =>
                {
                    databaseNameFactoryCalled = true;
                    return "neo4j";
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("neo4j");
        check.ShouldBeOfType<Neo4jHealthCheck>();
        driverFactoryCalled.ShouldBeTrue();
        databaseNameFactoryCalled.ShouldBeTrue();
    }
}
