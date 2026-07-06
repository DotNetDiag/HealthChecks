using Apache.NMS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ActiveMQ;

/// <summary>
/// A health check for Apache ActiveMQ Classic brokers.
/// </summary>
public sealed class ActiveMQHealthCheck : IHealthCheck
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ActiveMQHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="ActiveMQHealthCheck"/>.
    /// </summary>
    /// <param name="connectionFactory">The ActiveMQ connection factory used to connect to the broker.</param>
    /// <param name="options">Options for the ActiveMQ health check.</param>
    public ActiveMQHealthCheck(IConnectionFactory connectionFactory, ActiveMQHealthCheckOptions? options = null)
    {
        _connectionFactory = Guard.ThrowIfNull(connectionFactory);
        _options = options ?? new ActiveMQHealthCheckOptions();
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            using IConnection connection = CreateConnection();

            if (_options.RequestTimeout.HasValue)
            {
                connection.RequestTimeout = _options.RequestTimeout.Value;
            }

            using ISession session = connection.CreateSession(_options.AcknowledgementMode);
            connection.Start();

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }

    private IConnection CreateConnection() =>
        string.IsNullOrEmpty(_options.UserName) && string.IsNullOrEmpty(_options.Password)
            ? _connectionFactory.CreateConnection()
            : _connectionFactory.CreateConnection(_options.UserName, _options.Password);
}
