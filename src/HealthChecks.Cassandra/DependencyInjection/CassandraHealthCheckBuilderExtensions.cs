using Cassandra;
using HealthChecks.Cassandra;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="CassandraHealthCheck"/>.
/// </summary>
public static class CassandraHealthCheckBuilderExtensions
{
    private const string NAME = "cassandra";

    /// <summary>
    /// Add a health check for Cassandra databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clusterFactory">A factory to build the Cassandra <see cref="ICluster"/> to use.</param>
    /// <param name="options">Optional options for the health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cassandra' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCassandra(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ICluster> clusterFactory,
        CassandraHealthCheckOptions? options = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(clusterFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new CassandraHealthCheck(clusterFactory(sp), options),
            failureStatus,
            tags,
            timeout));
    }
}
