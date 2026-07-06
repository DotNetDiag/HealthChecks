using DuckDB.NET.Data;
using HealthChecks.DuckDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="DuckDbHealthCheck"/>.
/// </summary>
public static class DuckDbHealthCheckBuilderExtensions
{
    private const string NAME = "duckdb";
    internal const string HEALTH_QUERY = "SELECT 1;";

    /// <summary>
    /// Add a health check for DuckDB databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The DuckDB connection string to be used.</param>
    /// <param name="healthQuery">The query to be executed.</param>
    /// <param name="configure">An optional action to allow additional DuckDB specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'duckdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDuckDb(
        this IHealthChecksBuilder builder,
        string connectionString,
        string healthQuery = HEALTH_QUERY,
        Action<DuckDBConnection>? configure = null,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddDuckDb(new DuckDbHealthCheckOptions(connectionString)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for DuckDB databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the DuckDB connection string to use.</param>
    /// <param name="healthQuery">The query to be executed.</param>
    /// <param name="configure">An optional action to allow additional DuckDB specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'duckdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDuckDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string healthQuery = HEALTH_QUERY,
        Action<DuckDBConnection>? configure = null,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new DuckDbHealthCheck(new DuckDbHealthCheckOptions(
                Guard.ThrowIfNull(connectionStringFactory(sp), throwOnEmptyString: true, paramName: nameof(connectionStringFactory)))
            {
                CommandText = healthQuery,
                Configure = configure,
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for DuckDB databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'duckdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDuckDb(
        this IHealthChecksBuilder builder,
        DuckDbHealthCheckOptions options,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new DuckDbHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for DuckDB databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="optionsFactory">A factory to build the DuckDB health check options to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'duckdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDuckDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, DuckDbHealthCheckOptions> optionsFactory,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(optionsFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new DuckDbHealthCheck(Guard.ThrowIfNull(optionsFactory(sp), paramName: nameof(optionsFactory))),
            failureStatus,
            tags,
            timeout));
    }
}
