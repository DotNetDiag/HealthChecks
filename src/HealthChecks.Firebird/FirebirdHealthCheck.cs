using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Firebird;

/// <summary>
/// A health check for Firebird databases.
/// </summary>
public sealed class FirebirdHealthCheck : IHealthCheck
{
    private readonly FirebirdHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="FirebirdHealthCheck"/>.
    /// </summary>
    /// <param name="options">Options for the Firebird health check.</param>
    public FirebirdHealthCheck(FirebirdHealthCheckOptions options)
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
            using var connection = new FbConnection(_options.ConnectionString);

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
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }
}
