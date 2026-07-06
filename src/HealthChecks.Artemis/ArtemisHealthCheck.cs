using Apache.NMS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Artemis;

/// <summary>
/// A health check for Apache ActiveMQ Artemis brokers.
/// </summary>
public sealed class ArtemisHealthCheck : IHealthCheck
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ArtemisHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="ArtemisHealthCheck"/>.
    /// </summary>
    /// <param name="connectionFactory">The Artemis connection factory used to connect to the broker.</param>
    /// <param name="options">Options for the Artemis health check.</param>
    public ArtemisHealthCheck(IConnectionFactory connectionFactory, ArtemisHealthCheckOptions? options = null)
    {
        _connectionFactory = Guard.ThrowIfNull(connectionFactory);
        _options = options ?? new ArtemisHealthCheckOptions();
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
