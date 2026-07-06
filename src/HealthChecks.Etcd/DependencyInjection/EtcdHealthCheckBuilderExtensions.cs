using dotnet_etcd;
using HealthChecks.Etcd;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="EtcdHealthCheck"/>.
/// </summary>
public static class EtcdHealthCheckBuilderExtensions
{
    private const string NAME = "etcd";

    /// <summary>
    /// Add a health check for etcd services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The etcd connection string.</param>
    /// <param name="setup">An optional action to configure etcd health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'etcd' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddEtcd(
        this IHealthChecksBuilder builder,
        string connectionString,
        Action<EtcdHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString);

        return builder.AddEtcd(
            connectionStringFactory: _ => connectionString,
            optionsFactory: _ =>
            {
                var options = new EtcdHealthCheckOptions();
                setup?.Invoke(options);
                return options;
            },
            name: name,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout);
    }

    /// <summary>
    /// Add a health check for etcd services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">
    /// An optional factory to obtain the etcd connection string.
    /// When not provided, a registered <see cref="string"/> instance is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="EtcdHealthCheckOptions"/> used by the health check.
    /// When not provided, the etcd endpoint is checked with default client settings.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'etcd' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddEtcd(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string>? connectionStringFactory = default,
        Func<IServiceProvider, EtcdHealthCheckOptions>? optionsFactory = default,
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

    /// <summary>
    /// Add a health check for etcd services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="client">The etcd client.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'etcd' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddEtcd(
        this IHealthChecksBuilder builder,
        EtcdClient client,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(client);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new EtcdHealthCheck(client),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for etcd services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">A factory to obtain the etcd client.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'etcd' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddEtcd(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, EtcdClient> clientFactory,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(clientFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new EtcdHealthCheck(clientFactory(sp)),
            failureStatus,
            tags,
            timeout));
    }

    private static EtcdHealthCheck CreateHealthCheck(
        IServiceProvider sp,
        Func<IServiceProvider, string>? connectionStringFactory,
        Func<IServiceProvider, EtcdHealthCheckOptions>? optionsFactory)
    {
        string connectionString = connectionStringFactory?.Invoke(sp) ?? sp.GetRequiredService<string>();
        return new EtcdHealthCheck(connectionString, optionsFactory?.Invoke(sp));
    }
}
