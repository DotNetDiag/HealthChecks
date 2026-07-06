using HealthChecks.ZooKeeper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ZooKeeperHealthCheck"/>.
/// </summary>
public static class ZooKeeperHealthCheckBuilderExtensions
{
    private const string NAME = "zookeeper";

    /// <summary>
    /// Add a health check for Apache ZooKeeper.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The ZooKeeper connection string.</param>
    /// <param name="setup">An optional action to configure ZooKeeper health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'zookeeper' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddZooKeeper(
        this IHealthChecksBuilder builder,
        string connectionString,
        Action<ZooKeeperHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString);

        return builder.AddZooKeeper(
            connectionStringFactory: _ => connectionString,
            optionsFactory: _ =>
            {
                var options = new ZooKeeperHealthCheckOptions();
                setup?.Invoke(options);
                return options;
            },
            name: name,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout);
    }

    /// <summary>
    /// Add a health check for Apache ZooKeeper.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">
    /// An optional factory to obtain the ZooKeeper connection string.
    /// When not provided, a registered <see cref="string"/> instance is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="ZooKeeperHealthCheckOptions"/> used by the health check.
    /// When not provided, the ZooKeeper root znode is checked with the default session timeout.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'zookeeper' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddZooKeeper(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string>? connectionStringFactory = default,
        Func<IServiceProvider, ZooKeeperHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        string registrationName = name ?? NAME;

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => CreateHealthCheck(sp, connectionStringFactory, optionsFactory),
            failureStatus,
            tags,
            timeout));
    }

    private static ZooKeeperHealthCheck CreateHealthCheck(
        IServiceProvider sp,
        Func<IServiceProvider, string>? connectionStringFactory,
        Func<IServiceProvider, ZooKeeperHealthCheckOptions>? optionsFactory)
    {
        string connectionString = connectionStringFactory?.Invoke(sp) ?? sp.GetRequiredService<string>();
        return new ZooKeeperHealthCheck(connectionString, optionsFactory?.Invoke(sp));
    }
}
