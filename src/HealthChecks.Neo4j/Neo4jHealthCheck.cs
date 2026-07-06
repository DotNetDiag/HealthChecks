using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4j.Driver;

namespace HealthChecks.Neo4j;

/// <summary>
/// A health check for Neo4j services.
/// </summary>
public sealed class Neo4jHealthCheck : IHealthCheck
{
    private readonly IDriver _driver;
    private readonly string? _databaseName;

    /// <summary>
    /// Creates a new instance of <see cref="Neo4jHealthCheck"/>.
    /// </summary>
    /// <param name="driver">The Neo4j driver used to verify connectivity.</param>
    /// <param name="databaseName">An optional Neo4j database name to query.</param>
    public Neo4jHealthCheck(IDriver driver, string? databaseName = default)
    {
        _driver = Guard.ThrowIfNull(driver);
        _databaseName = databaseName;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                await _driver.VerifyConnectivityAsync().ConfigureAwait(false);
            }
            else
            {
                await using var session = _driver.AsyncSession(SessionConfigBuilder.ForDatabase(_databaseName));
                var cursor = await session.RunAsync("RETURN 1").ConfigureAwait(false);
                await cursor.ConsumeAsync().ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
