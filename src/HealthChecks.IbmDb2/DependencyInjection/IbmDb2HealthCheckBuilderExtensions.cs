using HealthChecks.IbmDb2;
using IBM.Data.Db2;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="IbmDb2HealthCheck"/>.
/// </summary>
public static class IbmDb2HealthCheckBuilderExtensions
{
    private const string NAME = "ibmdb2";

    internal const string HEALTH_QUERY = "SELECT 1 FROM SYSIBM.SYSDUMMY1";

    /// <summary>
    /// Add a health check for IBM Db2 databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The IBM Db2 connection string to use.</param>
    /// <param name="healthQuery">The query to execute.</param>
    /// <param name="configure">An optional action to allow additional IBM Db2 specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ibmdb2' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddIbmDb2(
        this IHealthChecksBuilder builder,
        string connectionString,
        string healthQuery = HEALTH_QUERY,
        Action<DB2Connection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddIbmDb2(_ => connectionString, healthQuery, configure, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for IBM Db2 databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the IBM Db2 connection string to use.</param>
    /// <param name="healthQuery">The query to execute.</param>
    /// <param name="configure">An optional action to allow additional IBM Db2 specific configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ibmdb2' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddIbmDb2(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string healthQuery = HEALTH_QUERY,
        Action<DB2Connection>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new IbmDb2HealthCheck(new IbmDb2HealthCheckOptions
            {
                ConnectionString = connectionStringFactory(sp),
                CommandText = healthQuery,
                Configure = configure,
            }),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for IBM Db2 databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ibmdb2' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddIbmDb2(
        this IHealthChecksBuilder builder,
        IbmDb2HealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new IbmDb2HealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
