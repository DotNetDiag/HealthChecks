using IBM.Data.Db2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IbmDb2;

/// <summary>
/// Represents settings used by <see cref="IbmDb2HealthCheck"/>.
/// </summary>
public sealed class IbmDb2HealthCheckOptions
{
    /// <summary>
    /// Gets or sets the IBM Db2 connection string to use.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the query to execute.
    /// </summary>
    public string CommandText { get; set; } = IbmDb2HealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// Gets or sets an optional callback executed before the connection is opened.
    /// </summary>
    public Action<DB2Connection>? Configure { get; set; }

    /// <summary>
    /// Gets or sets an optional delegate to build the health check result from the query result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
