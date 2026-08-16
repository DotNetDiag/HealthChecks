using DuckDB.NET.Data;

namespace HealthChecks.DuckDb.Tests.DependencyInjection;

public class duckdb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDuckDb("Data Source=:memory:");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("duckdb");
        check.ShouldBeOfType<DuckDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_connection_string_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDuckDb(_ => "Data Source=:memory:");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("duckdb");
        check.ShouldBeOfType<DuckDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_options()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDuckDb(new DuckDbHealthCheckOptions("Data Source=:memory:"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("duckdb");
        check.ShouldBeOfType<DuckDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_options_factory()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDuckDb(_ => new DuckDbHealthCheckOptions("Data Source=:memory:"));

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("duckdb");
        check.ShouldBeOfType<DuckDbHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDuckDb("Data Source=:memory:", name: "my-duckdb");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-duckdb");
        check.ShouldBeOfType<DuckDbHealthCheck>();
    }

    [Fact]
    public async Task invoke_configure_when_defined()
    {
        var services = new ServiceCollection();
        bool invoked = false;
        const string connectionString = "Data Source=:memory:";

        void Configure(DuckDBConnection connection)
        {
            invoked = true;
            connection.ConnectionString.ShouldBe(connectionString);
        }

        services.AddHealthChecks()
            .AddDuckDb(connectionString, configure: Configure);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.First();
        IHealthCheck check = registration.Factory(serviceProvider);

        await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });

        invoked.ShouldBeTrue();
    }
}
