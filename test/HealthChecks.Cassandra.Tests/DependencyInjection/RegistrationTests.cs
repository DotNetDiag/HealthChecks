using Cassandra;

namespace HealthChecks.Cassandra.Tests.DependencyInjection;

public class cassandra_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCassandra(static _ => Cluster.Builder().AddContactPoint("localhost").Build());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("cassandra");
        check.ShouldBeOfType<CassandraHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddCassandra(static _ => Cluster.Builder().AddContactPoint("localhost").Build(), name: "my-cassandra-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-cassandra-1");
        check.ShouldBeOfType<CassandraHealthCheck>();
    }
}
