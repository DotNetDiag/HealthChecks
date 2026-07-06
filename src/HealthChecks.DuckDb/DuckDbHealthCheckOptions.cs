using DuckDB.NET.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DuckDb;

/// <summary>
/// Options for <see cref="DuckDbHealthCheck"/>.
/// </summary>
public sealed class DuckDbHealthCheckOptions
{
    /// <summary>
    /// Creates an instance of <see cref="DuckDbHealthCheckOptions"/>.
    /// </summary>
    public DuckDbHealthCheckOptions()
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="DuckDbHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The DuckDB connection string to be used.</param>
    public DuckDbHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// The DuckDB connection string to be used.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = DuckDbHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<DuckDBConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
