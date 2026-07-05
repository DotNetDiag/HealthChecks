using HealthChecks.SurrealDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SurrealDbHealthCheck"/>.
/// </summary>
public static class SurrealDbHealthCheckBuilderExtensions
{
    private const string NAME = "surrealdb";

    /// <summary>
    /// Add a health check for SurrealDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="factory">
    /// An optional factory to obtain <see cref="ISurrealDbClient" /> instance.
    /// When not provided, <see cref="ISurrealDbClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'surrealdb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSurreal(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ISurrealDbClient>? factory = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => Factory(sp, factory),
            failureStatus,
            tags,
            timeout));

        static SurrealDbHealthCheck Factory(IServiceProvider sp, Func<IServiceProvider, ISurrealDbClient>? factory)
        {
            // SurrealDb.Net registers clients for singleton lifetime and sessions for scoped/transient lifetimes.
            ISurrealDbSharedMethods? client = factory?.Invoke(sp)
                ?? sp.GetService<ISurrealDbClient>();
            client ??= sp.GetService<SurrealDbClient>();

            if (client is not null)
            {
                return new(client);
            }

            if (sp.GetService<IServiceProviderIsService>() is { } serviceProviderIsService
                && (serviceProviderIsService.IsService(typeof(ISurrealDbSession))
                    || serviceProviderIsService.IsService(typeof(SurrealDbSession))))
            {
                return new(sp.GetRequiredService<IServiceScopeFactory>());
            }

            return new(sp.GetRequiredService<SurrealDbClient>());
        }
    }
}
