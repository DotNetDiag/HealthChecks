using HealthChecks.Neo4j;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4j.Driver;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="Neo4jHealthCheck"/>.
/// </summary>
public static class Neo4jHealthCheckBuilderExtensions
{
    private const string NAME = "neo4j";

    /// <summary>
    /// Add a health check for Neo4j.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="driverFactory">
    /// An optional factory to obtain <see cref="IDriver" /> instance.
    /// When not provided, <see cref="IDriver" /> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="databaseNameFactory">An optional factory to obtain the name of the database to query.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'neo4j' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNeo4j(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IDriver>? driverFactory = default,
        Func<IServiceProvider, string?>? databaseNameFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new Neo4jHealthCheck(
                driverFactory?.Invoke(sp) ?? sp.GetRequiredService<IDriver>(),
                databaseNameFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
