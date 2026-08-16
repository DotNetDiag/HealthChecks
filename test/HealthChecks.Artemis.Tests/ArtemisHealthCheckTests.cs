using Apache.NMS;
using NSubstitute;

namespace HealthChecks.Artemis.Tests;

public class artemis_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_connection_and_session_can_be_created()
    {
        var connectionFactory = Substitute.For<IConnectionFactory>();
        var connection = Substitute.For<IConnection>();
        var session = Substitute.For<ISession>();
        var timeout = TimeSpan.FromSeconds(5);
        connectionFactory.CreateConnection().Returns(connection);
        connection.CreateSession(AcknowledgementMode.ClientAcknowledge).Returns(session);

        var healthCheck = new ArtemisHealthCheck(
            connectionFactory,
            new ArtemisHealthCheckOptions
            {
                AcknowledgementMode = AcknowledgementMode.ClientAcknowledge,
                RequestTimeout = timeout
            });

        HealthCheckResult result = await healthCheck.CheckHealthAsync(CreateContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        connectionFactory.Received(1).CreateConnection();
        connection.Received(1).RequestTimeout = timeout;
        connection.Received(1).CreateSession(AcknowledgementMode.ClientAcknowledge);
        connection.Received(1).Start();
    }

    [Fact]
    public async Task use_credentials_when_user_name_or_password_is_configured()
    {
        var connectionFactory = Substitute.For<IConnectionFactory>();
        var connection = Substitute.For<IConnection>();
        var session = Substitute.For<ISession>();
        connectionFactory.CreateConnection("artemis", "artemis").Returns(connection);
        connection.CreateSession(AcknowledgementMode.AutoAcknowledge).Returns(session);

        var healthCheck = new ArtemisHealthCheck(
            connectionFactory,
            new ArtemisHealthCheckOptions
            {
                UserName = "artemis",
                Password = "artemis"
            });

        HealthCheckResult result = await healthCheck.CheckHealthAsync(CreateContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        connectionFactory.Received(1).CreateConnection("artemis", "artemis");
        connectionFactory.DidNotReceive().CreateConnection();
    }

    [Fact]
    public async Task be_unhealthy_when_connection_can_not_be_created()
    {
        var connectionFactory = Substitute.For<IConnectionFactory>();
        var exception = new InvalidOperationException("Cannot connect.");
        connectionFactory.CreateConnection().Returns(_ => throw exception);
        var healthCheck = new ArtemisHealthCheck(connectionFactory);

        HealthCheckResult result = await healthCheck.CheckHealthAsync(CreateContext(HealthStatus.Degraded));

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Exception.ShouldBe(exception);
    }

    [Fact]
    public void throw_when_connection_factory_is_null()
    {
        Action action = () => new ArtemisHealthCheck(null!);

        action.ShouldThrow<ArgumentNullException>();
    }

    private static HealthCheckContext CreateContext(HealthStatus failureStatus = HealthStatus.Unhealthy) =>
        new()
        {
            Registration = new HealthCheckRegistration("artemis", _ => Substitute.For<IHealthCheck>(), failureStatus, [])
        };
}
