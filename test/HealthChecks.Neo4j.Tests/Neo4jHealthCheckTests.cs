using Neo4j.Driver;
using NSubstitute;

namespace HealthChecks.Neo4j.Tests;

public class neo4j_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_driver_connectivity_succeeds()
    {
        var driver = Substitute.For<IDriver>();
        driver.VerifyConnectivityAsync().Returns(Task.CompletedTask);
        var healthCheck = new Neo4jHealthCheck(driver);

        var result = await healthCheck.CheckHealthAsync(CreateContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        await driver.Received(1).VerifyConnectivityAsync();
    }

    [Fact]
    public async Task be_healthy_when_database_query_succeeds()
    {
        var driver = Substitute.For<IDriver>();
        var session = Substitute.For<IAsyncSession>();
        var cursor = Substitute.For<IResultCursor>();
        cursor.ConsumeAsync().Returns(Substitute.For<IResultSummary>());
        driver.AsyncSession(Arg.Any<Action<SessionConfigBuilder>>()).Returns(session);
        session.RunAsync("RETURN 1").Returns(cursor);
        var healthCheck = new Neo4jHealthCheck(driver, "neo4j");

        var result = await healthCheck.CheckHealthAsync(CreateContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        driver.Received(1).AsyncSession(Arg.Any<Action<SessionConfigBuilder>>());
        await session.Received(1).RunAsync("RETURN 1");
        await cursor.Received(1).ConsumeAsync();
    }

    [Fact]
    public async Task be_unhealthy_when_driver_connectivity_fails()
    {
        var driver = Substitute.For<IDriver>();
        var exception = new InvalidOperationException("Cannot connect.");
        driver.VerifyConnectivityAsync().Returns(Task.FromException(exception));
        var healthCheck = new Neo4jHealthCheck(driver);

        var result = await healthCheck.CheckHealthAsync(CreateContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Exception.ShouldBe(exception);
    }

    private static HealthCheckContext CreateContext() =>
        new()
        {
            Registration = new HealthCheckRegistration("neo4j", _ => Substitute.For<IHealthCheck>(), HealthStatus.Unhealthy, [])
        };
}
