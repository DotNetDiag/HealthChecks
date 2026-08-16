using DuckDB.NET.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DuckDb;

/// <summary>
/// A health check for DuckDB databases.
/// </summary>
public sealed class DuckDbHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly DuckDbHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="DuckDbHealthCheck"/>.
    /// </summary>
    /// <param name="options">Options for the DuckDB health check.</param>
    public DuckDbHealthCheck(DuckDbHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options);
        _connectionString = Guard.ThrowIfNull(options.ConnectionString, throwOnEmptyString: true);
        Guard.ThrowIfNull(options.CommandText, throwOnEmptyString: true);

        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new DuckDBConnection(_connectionString);

            _options.Configure?.Invoke(connection);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
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
