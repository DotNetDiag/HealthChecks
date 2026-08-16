using HealthChecks.IoTDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="IoTDBHealthCheck"/>.
/// </summary>
public static class IoTDBHealthCheckBuilderExtensions
{
    private const string NAME = "iotdb";

    /// <summary>
    /// Add a health check for Apache IoTDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The IoTDB connection string.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iotdb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddIoTDB(
        this IHealthChecksBuilder builder,
        string connectionString,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddIoTDB(new IoTDBHealthCheckOptions { ConnectionString = connectionString }, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Apache IoTDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">The <see cref="IoTDBHealthCheckOptions"/> used for the health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'iotdb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddIoTDB(
        this IHealthChecksBuilder builder,
        IoTDBHealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new IoTDBHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
