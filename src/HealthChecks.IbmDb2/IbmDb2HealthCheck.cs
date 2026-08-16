using IBM.Data.Db2;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IbmDb2;

/// <summary>
/// A health check for IBM Db2 databases.
/// </summary>
public sealed class IbmDb2HealthCheck : IHealthCheck
{
    private readonly IbmDb2HealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the IBM Db2 health check.
    /// </summary>
    /// <param name="options">Options used by the health check.</param>
    public IbmDb2HealthCheck(IbmDb2HealthCheckOptions options)
    {
        Guard.ThrowIfNull(options);
        Guard.ThrowIfNull(options.ConnectionString, throwOnEmptyString: true);
        Guard.ThrowIfNull(options.CommandText, throwOnEmptyString: true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new DB2Connection(_options.ConnectionString);

            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = _options.CommandText;
            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder is null
                ? HealthCheckResult.Healthy()
                : _options.HealthCheckResultBuilder(result);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
