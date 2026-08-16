using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace HealthChecks.SurrealDb;

/// <summary>
/// A health check for SurrealDb services.
/// </summary>
public class SurrealDbHealthCheck : IHealthCheck
{
    private readonly ISurrealDbSharedMethods? _client;
    private readonly IServiceScopeFactory? _scopeFactory;

    public SurrealDbHealthCheck(ISurrealDbClient client)
        : this((ISurrealDbSharedMethods)client)
    {
    }

    internal SurrealDbHealthCheck(ISurrealDbSharedMethods client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    internal SurrealDbHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = Guard.ThrowIfNull(scopeFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _client is not null
            ? await CheckHealthAsync(_client, context, cancellationToken).ConfigureAwait(false)
            : await CheckScopedHealthAsync(context, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HealthCheckResult> CheckHealthAsync(
        ISurrealDbSharedMethods client,
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.Health(cancellationToken).ConfigureAwait(false)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<HealthCheckResult> CheckScopedHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        AsyncServiceScope scope = _scopeFactory!.CreateAsyncScope();

        try
        {
            ISurrealDbSharedMethods client = scope.ServiceProvider.GetService<ISurrealDbSession>()
                ?? scope.ServiceProvider.GetRequiredService<SurrealDbSession>();

            return await CheckHealthAsync(client, context, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                await scope.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // SurrealDb.Net scoped HTTP sessions can fail while closing after the health result is known.
            }
        }
    }
}
