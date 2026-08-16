using Enyim.Caching;
using HealthChecks.Memcached;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using MicrosoftNullLoggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="MemcachedHealthCheck"/>.
/// </summary>
public static class MemcachedHealthCheckBuilderExtensions
{
    private const string NAME = "memcached";

    /// <summary>
    /// Add a health check for Memcached services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="server">The Memcached server address.</param>
    /// <param name="port">The Memcached server port.</param>
    /// <param name="setup">An optional action to configure Memcached health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'memcached' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMemcached(
        this IHealthChecksBuilder builder,
        string server,
        int port = MemcachedHealthCheckOptions.DEFAULT_PORT,
        Action<MemcachedHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(server);

        return builder.AddMemcached(
            options =>
            {
                options.AddServer(server, port);
                setup?.Invoke(options);
            },
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Memcached services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The Memcached health check option setup.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'memcached' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMemcached(
        this IHealthChecksBuilder builder,
        Action<MemcachedHealthCheckOptions> setup,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(setup);

        var options = new MemcachedHealthCheckOptions();
        setup(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MemcachedHealthCheck(options, sp.GetService<ILoggerFactory>() ?? MicrosoftNullLoggerFactory.Instance),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Memcached services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="memcachedClient">The Memcached client to be used.</param>
    /// <param name="options">Optional settings used by the health check operation.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'memcached' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMemcached(
        this IHealthChecksBuilder builder,
        IMemcachedClient memcachedClient,
        MemcachedHealthCheckOptions? options = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(memcachedClient);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new MemcachedHealthCheck(memcachedClient, options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Memcached services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">A factory to obtain the Memcached client.</param>
    /// <param name="optionsFactory">An optional factory to obtain <see cref="MemcachedHealthCheckOptions"/> used by the health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'memcached' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMemcached(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IMemcachedClient>? clientFactory = default,
        Func<IServiceProvider, MemcachedHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var registrationName = name ?? NAME;
        var syncLock = new object();
        MemcachedHealthCheck? healthCheck = null;

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp =>
            {
                if (healthCheck is not null)
                {
                    return healthCheck;
                }

                lock (syncLock)
                {
                    healthCheck ??= new MemcachedHealthCheck(
                        clientFactory?.Invoke(sp) ?? sp.GetRequiredService<IMemcachedClient>(),
                        optionsFactory?.Invoke(sp));
                }

                return healthCheck;
            },
            failureStatus,
            tags,
            timeout));
    }
}
