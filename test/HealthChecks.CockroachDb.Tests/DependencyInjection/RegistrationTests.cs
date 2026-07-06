using Npgsql;

namespace HealthChecks.CockroachDb.Tests.DependencyInjection;

public class cockroachdb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCockroachDb("Host=localhost");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("cockroachdb");
        check.ShouldBeOfType<CockroachDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string_factory()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddCockroachDb(_ =>
            {
                factoryCalled = true;
                return "Host=localhost";
            });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("cockroachdb");
        check.ShouldBeOfType<CockroachDbHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task add_health_check_when_properly_configured_with_data_source()
    {
        await using NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder("Host=localhost").Build();

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCockroachDb(dataSource);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("cockroachdb");
        check.ShouldBeOfType<CockroachDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_registered_data_source()
    {
        var services = new ServiceCollection();
        services.AddNpgsqlDataSource("Host=pg_server;Username=root;Database=defaultdb");
        services.AddHealthChecks().AddCockroachDb();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.Single();
        IHealthCheck check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<CockroachDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_node_health_endpoint()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCockroachDb(new Uri("http://localhost:8080/health?ready=1"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("cockroachdb");
        check.ShouldBeOfType<CockroachDbHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCockroachDb("Host=localhost", name: "my-cockroachdb-cluster");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-cockroachdb-cluster");
        check.ShouldBeOfType<CockroachDbHealthCheck>();
    }
}
