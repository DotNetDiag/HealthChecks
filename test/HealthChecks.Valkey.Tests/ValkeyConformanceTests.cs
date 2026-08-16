using NSubstitute;
using StackExchange.Redis;

namespace HealthChecks.Valkey.Tests;

public class ValkeyConformanceTests
{
    [Fact]
    public void health_check_type_is_sealed() => typeof(ValkeyHealthCheck).IsSealed.ShouldBeTrue();

    [Fact]
    public void ctor_throws_argument_null_exception_for_null_connection_multiplexer()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new ValkeyHealthCheck(connectionMultiplexer: null!));

        argumentNullException.ParamName.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(HealthStatus.Unhealthy)]
    [InlineData(HealthStatus.Degraded)]
    public async Task returns_provided_failure_status_when_connection_fails(HealthStatus failureStatus)
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        connectionMultiplexer.GetEndPoints(configuredOnly: true).Returns(_ => throw new InvalidOperationException("Valkey unavailable."));

        var healthCheck = new ValkeyHealthCheck(connectionMultiplexer);
        var registration = new HealthCheckRegistration("valkey", healthCheck, failureStatus, tags: null);
        var context = new HealthCheckContext { Registration = registration };

        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(failureStatus);
    }
}

