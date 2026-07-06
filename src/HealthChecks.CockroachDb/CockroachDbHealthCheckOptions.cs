using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HealthChecks.CockroachDb;

/// <summary>
/// Options for <see cref="CockroachDbHealthCheck"/>.
/// </summary>
public sealed class CockroachDbHealthCheckOptions
{
    internal CockroachDbHealthCheckOptions()
    {
        // This ctor is internal on purpose: public construction should specify
        // either a SQL connection/data source or a CockroachDB node health endpoint.
    }

    /// <summary>
    /// Creates an instance of <see cref="CockroachDbHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The CockroachDB connection string to be used.</param>
    public CockroachDbHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// Creates an instance of <see cref="CockroachDbHealthCheckOptions"/>.
    /// </summary>
    /// <param name="dataSource">The CockroachDB <see cref="NpgsqlDataSource" /> to be used.</param>
    /// <remarks>
    /// Depending on how the <see cref="NpgsqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="NpgsqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public CockroachDbHealthCheckOptions(NpgsqlDataSource dataSource)
    {
        DataSource = Guard.ThrowIfNull(dataSource);
    }

    /// <summary>
    /// Creates an instance of <see cref="CockroachDbHealthCheckOptions"/>.
    /// </summary>
    /// <param name="nodeHealthEndpoint">The CockroachDB node health endpoint to be used.</param>
    public CockroachDbHealthCheckOptions(Uri nodeHealthEndpoint)
    {
        NodeHealthEndpoint = Guard.ThrowIfNull(nodeHealthEndpoint);
    }

    /// <summary>
    /// The CockroachDB connection string to be used.
    /// </summary>
    public string? ConnectionString { get; internal set; }

    /// <summary>
    /// The CockroachDB <see cref="NpgsqlDataSource" /> to be used.
    /// </summary>
    public NpgsqlDataSource? DataSource { get; internal set; }

    /// <summary>
    /// The query to be executed against CockroachDB.
    /// </summary>
    public string CommandText { get; set; } = CockroachDbHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// The optional CockroachDB node health endpoint to be checked.
    /// </summary>
    /// <remarks>
    /// Use a readiness endpoint such as <c>http://host:8080/health?ready=1</c> when the check should fail for draining,
    /// decommissioning, or unavailable nodes.
    /// </remarks>
    public Uri? NodeHealthEndpoint { get; set; }

    /// <summary>
    /// An optional action executed before the SQL connection is opened in the health check.
    /// </summary>
    public Action<NpgsqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build the SQL health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
