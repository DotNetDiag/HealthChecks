using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;
using OpenSearch.Net;

namespace HealthChecks.OpenSearch;

/// <summary>
/// A health check for OpenSearch services.
/// </summary>
public class OpenSearchHealthCheck : IHealthCheck
{
    private readonly IOpenSearchClient _client;
    private readonly bool _useClusterHealthApi;

    public OpenSearchHealthCheck(IOpenSearchClient client, bool useClusterHealthApi = false)
    {
        _client = Guard.ThrowIfNull(client);
        _useClusterHealthApi = useClusterHealthApi;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_useClusterHealthApi)
            {
                var healthResponse = await _client.Cluster.HealthAsync(new ClusterHealthRequest(), cancellationToken).ConfigureAwait(false);

                if (!healthResponse.IsValid)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: healthResponse.OriginalException);
                }

                return healthResponse.Status switch
                {
                    Health.Green => HealthCheckResult.Healthy(),
                    Health.Yellow => HealthCheckResult.Degraded(),
                    _ => new HealthCheckResult(context.Registration.FailureStatus)
                };
            }

            var pingResponse = await _client.PingAsync(new PingRequest(), cancellationToken).ConfigureAwait(false);

            return pingResponse.IsValid
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, exception: pingResponse.OriginalException);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
