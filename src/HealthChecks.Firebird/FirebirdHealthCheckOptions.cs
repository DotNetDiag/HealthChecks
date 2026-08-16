using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Firebird;

/// <summary>
/// Options for <see cref="FirebirdHealthCheck"/>.
/// </summary>
public sealed class FirebirdHealthCheckOptions
{
    /// <summary>
    /// Creates an instance of <see cref="FirebirdHealthCheckOptions"/>.
    /// </summary>
    public FirebirdHealthCheckOptions()
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="FirebirdHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The Firebird connection string to be used.</param>
    public FirebirdHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// The Firebird connection string to be used.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = FirebirdHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<FbConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
