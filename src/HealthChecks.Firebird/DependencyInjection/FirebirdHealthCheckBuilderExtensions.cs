using FirebirdSql.Data.FirebirdClient;
using HealthChecks.Firebird;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="FirebirdHealthCheck"/>.
/// </summary>
public static class FirebirdHealthCheckBuilderExtensions
{
    private const string NAME = "firebird";
    internal const string HEALTH_QUERY = "SELECT 1 FROM RDB$DATABASE";

    /// <summary>
    /// Add a health check for Firebird databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Firebird connection string to be used.</param>
    /// <param name="healthQuery">The query to be executed.</param>
    /// <param name="configure">An optional action to allow additional Firebird specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'firebird' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFirebird(
        this IHealthChecksBuilder builder,
        string connectionString,
        string healthQuery = HEALTH_QUERY,
        Action<FbConnection>? configure = null,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddFirebird(new FirebirdHealthCheckOptions(connectionString)
        {
            CommandText = healthQuery,
            Configure = configure
        }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Firebird databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Firebird connection string to use.</param>
    /// <param name="healthQuery">The query to be executed.</param>
    /// <param name="configure">An optional action to allow additional Firebird specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'firebird' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFirebird(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string healthQuery = HEALTH_QUERY,
        Action<FbConnection>? configure = null,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new FirebirdHealthCheck(new FirebirdHealthCheckOptions(connectionStringFactory(sp))
            {
                CommandText = healthQuery,
                Configure = configure,
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Firebird databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'firebird' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFirebird(
        this IHealthChecksBuilder builder,
        FirebirdHealthCheckOptions options,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new FirebirdHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Firebird databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="optionsFactory">A factory to build the Firebird health check options to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'firebird' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFirebird(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, FirebirdHealthCheckOptions> optionsFactory,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(optionsFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new FirebirdHealthCheck(optionsFactory(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
