using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HealthChecks.CockroachDb;

/// <summary>
/// A health check for CockroachDB SQL connectivity and node health endpoints.
/// </summary>
public sealed class CockroachDbHealthCheck : IHealthCheck
{
    private static readonly HttpClient _sharedHttpClient = new();

    private readonly HttpClient _httpClient;
    private readonly CockroachDbHealthCheckOptions _options;

    /// <summary>
    /// Creates an instance of <see cref="CockroachDbHealthCheck"/>.
    /// </summary>
    /// <param name="options">Options for health check.</param>
    /// <param name="httpClient">The optional HTTP client used for node health endpoint checks.</param>
    public CockroachDbHealthCheck(CockroachDbHealthCheckOptions options, HttpClient? httpClient = null)
    {
        _options = Guard.ThrowIfNull(options);
        _httpClient = httpClient ?? _sharedHttpClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateOptions();

            HealthCheckResult? successfulSqlResult = null;

            if (HasSqlCheck)
            {
                HealthCheckResult sqlResult = await CheckSqlAsync(cancellationToken).ConfigureAwait(false);

                if (sqlResult.Status != HealthStatus.Healthy)
                {
                    return sqlResult;
                }

                successfulSqlResult = sqlResult;
            }

            if (_options.NodeHealthEndpoint is not null)
            {
                HealthCheckResult? nodeResult = await CheckNodeHealthEndpointAsync(context, cancellationToken).ConfigureAwait(false);

                if (nodeResult.HasValue)
                {
                    return nodeResult.Value;
                }
            }

            return successfulSqlResult ?? HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }

    private bool HasSqlCheck => _options.DataSource is not null || _options.ConnectionString is not null;

    private async Task<HealthCheckResult> CheckSqlAsync(CancellationToken cancellationToken)
    {
        await using var connection = _options.DataSource is not null
            ? _options.DataSource.CreateConnection()
            : new NpgsqlConnection(_options.ConnectionString);

        _options.Configure?.Invoke(connection);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = _options.CommandText;
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        return _options.HealthCheckResultBuilder is null
            ? HealthCheckResult.Healthy()
            : _options.HealthCheckResultBuilder(result);
    }

    private async Task<HealthCheckResult?> CheckNodeHealthEndpointAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        Uri nodeHealthEndpoint = _options.NodeHealthEndpoint!;

        using HttpResponseMessage response = await _httpClient.GetAsync(nodeHealthEndpoint, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        string description = $"CockroachDB node health endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).";

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            description: description,
            data: new Dictionary<string, object>
            {
                { "endpoint", nodeHealthEndpoint.ToString() },
                { "statusCode", (int)response.StatusCode }
            });
    }

    private void ValidateOptions()
    {
        if (!HasSqlCheck && _options.NodeHealthEndpoint is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CockroachDbHealthCheckOptions.ConnectionString)}, {nameof(CockroachDbHealthCheckOptions.DataSource)}, or {nameof(CockroachDbHealthCheckOptions.NodeHealthEndpoint)} must be configured.");
        }

        if (HasSqlCheck)
        {
            Guard.ThrowIfNull(_options.CommandText, throwOnEmptyString: true, paramName: nameof(CockroachDbHealthCheckOptions.CommandText));
        }

        if (_options.NodeHealthEndpoint is { IsAbsoluteUri: false })
        {
            throw new InvalidOperationException($"{nameof(CockroachDbHealthCheckOptions.NodeHealthEndpoint)} must be an absolute URI.");
        }
    }
}
