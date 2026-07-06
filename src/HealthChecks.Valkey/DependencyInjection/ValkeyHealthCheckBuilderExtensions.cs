using HealthChecks.Valkey;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ValkeyHealthCheck"/>.
/// </summary>
public static class ValkeyHealthCheckBuilderExtensions
{
    private const string NAME = "valkey";

    /// <summary>
    /// Add a health check for Valkey services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="valkeyConnectionString">The Valkey connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'valkey' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddValkey(
        this IHealthChecksBuilder builder,
        string valkeyConnectionString,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(valkeyConnectionString, throwOnEmptyString: true);

        return builder.AddValkey(_ => valkeyConnectionString, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Valkey services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Valkey connection string to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'valkey' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddValkey(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ValkeyHealthCheck(connectionStringFactory(sp)),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Valkey services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionMultiplexer">The Valkey connection to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'valkey' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddValkey(
        this IHealthChecksBuilder builder,
        IConnectionMultiplexer connectionMultiplexer,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionMultiplexer);

        return builder.AddValkey(_ => connectionMultiplexer, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Valkey services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionMultiplexerFactory">A factory to build the Valkey connection to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'valkey' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddValkey(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IConnectionMultiplexer> connectionMultiplexerFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionMultiplexerFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ValkeyHealthCheck(() => connectionMultiplexerFactory(sp)),
            failureStatus,
            tags,
            timeout));
    }
}

