using Microsoft.Extensions.Diagnostics.HealthChecks;
using org.apache.zookeeper.data;

namespace HealthChecks.ZooKeeper;

/// <summary>
/// A health check for Apache ZooKeeper.
/// </summary>
public sealed class ZooKeeperHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ZooKeeperHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the ZooKeeper health check.
    /// </summary>
    /// <param name="connectionString">The ZooKeeper connection string.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public ZooKeeperHealthCheck(string connectionString, ZooKeeperHealthCheckOptions? options = default)
    {
        _connectionString = Guard.ThrowIfNull(connectionString);
        _options = options ?? new ZooKeeperHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateOptions();

            Stat? stat = await CheckPathExistsAsync(cancellationToken).ConfigureAwait(false);
            if (stat is null)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"ZooKeeper znode '{_options.Path}' does not exist.");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<Stat?> CheckPathExistsAsync(CancellationToken cancellationToken)
    {
        var sessionTimeoutMilliseconds = checked((int)_options.SessionTimeout.TotalMilliseconds);

        Task<Stat> checkTask = org.apache.zookeeper.ZooKeeper.Using(
            _connectionString,
            sessionTimeoutMilliseconds,
            _options.WatcherFactory(),
            zooKeeper => zooKeeper.existsAsync(_options.Path, watch: false),
            _options.CanBeReadOnly);

        return await checkTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException($"{nameof(_connectionString)} must be configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Path))
        {
            throw new InvalidOperationException($"{nameof(ZooKeeperHealthCheckOptions.Path)} must be configured.");
        }

        if (_options.SessionTimeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{nameof(ZooKeeperHealthCheckOptions.SessionTimeout)} must be greater than zero.");
        }

        if (_options.SessionTimeout.TotalMilliseconds > int.MaxValue)
        {
            throw new InvalidOperationException($"{nameof(ZooKeeperHealthCheckOptions.SessionTimeout)} must be less than or equal to {int.MaxValue} milliseconds.");
        }

        if (_options.WatcherFactory is null)
        {
            throw new InvalidOperationException($"{nameof(ZooKeeperHealthCheckOptions.WatcherFactory)} must be configured.");
        }
    }
}
