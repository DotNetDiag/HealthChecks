using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Vault;

/// <summary>
/// A health check for HashiCorp Vault.
/// </summary>
public sealed class VaultHealthCheck : IHealthCheck
{
    private const int ACTIVE_STATUS_CODE = 200;
    private const int STANDBY_STATUS_CODE = 429;
    private const int DISASTER_RECOVERY_SECONDARY_STATUS_CODE = 472;
    private const int PERFORMANCE_STANDBY_STATUS_CODE = 473;
    private const int HIGH_AVAILABILITY_UNHEALTHY_STATUS_CODE = 474;
    private const int UNINITIALIZED_STATUS_CODE = 501;
    private const int SEALED_STATUS_CODE = 503;
    private const int REMOVED_STATUS_CODE = 530;

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly VaultHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the Vault health check.
    /// </summary>
    /// <param name="client">The HTTP client used to call the Vault API.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public VaultHealthCheck(HttpClient client, VaultHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new VaultHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetHealthEndpointUri());
            _options.ConfigureRequest?.Invoke(request);

            using HttpResponseMessage response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            VaultHealthResponse? health = DeserializeResponse(responseContent);
            VaultHealthStatus vaultStatus = health is null
                ? GetVaultStatus(response.StatusCode)
                : GetVaultStatus(health, response.StatusCode);
            Dictionary<string, object> data = CreateData(response, health, vaultStatus);

            if (vaultStatus == VaultHealthStatus.Unknown)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"Vault health endpoint returned HTTP {(int)response.StatusCode} without a recognizable Vault health status.",
                    data: data);
            }

            if (!_options.HealthyStatuses.Contains(vaultStatus))
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"Vault reported status '{vaultStatus}' with HTTP {(int)response.StatusCode}.",
                    data: data);
            }

            return HealthCheckResult.Healthy(data: data);
        }
        catch (JsonException ex)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "Vault health endpoint returned invalid JSON.",
                exception: ex);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private Uri GetHealthEndpointUri()
    {
        if (string.IsNullOrWhiteSpace(_options.HealthEndpointPath))
        {
            throw new InvalidOperationException($"{nameof(VaultHealthCheckOptions.HealthEndpointPath)} must be configured.");
        }

        if (Uri.TryCreate(_options.HealthEndpointPath, UriKind.Absolute, out Uri? absoluteUri) && !absoluteUri.IsFile)
        {
            return absoluteUri;
        }

        Uri? baseUri = (_options.BaseUri ?? _client.BaseAddress) ?? throw new InvalidOperationException($"{nameof(VaultHealthCheckOptions.BaseUri)} or {nameof(HttpClient.BaseAddress)} must be configured.");

        return new Uri(baseUri, _options.HealthEndpointPath);
    }

    private static VaultHealthResponse? DeserializeResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return null;
        }

        return JsonSerializer.Deserialize<VaultHealthResponse>(responseContent, _serializerOptions);
    }

    private static VaultHealthStatus GetVaultStatus(VaultHealthResponse health, HttpStatusCode statusCode)
    {
        if (health.RemovedFromCluster == true)
        {
            return VaultHealthStatus.Removed;
        }

        if (health.Initialized == false)
        {
            return VaultHealthStatus.Uninitialized;
        }

        if (health.Sealed == true)
        {
            return VaultHealthStatus.Sealed;
        }

        if (health.HighAvailabilityConnectionHealthy == false)
        {
            return VaultHealthStatus.HighAvailabilityUnhealthy;
        }

        if (string.Equals(health.ReplicationDisasterRecoveryMode, "secondary", StringComparison.OrdinalIgnoreCase))
        {
            return VaultHealthStatus.DisasterRecoverySecondary;
        }

        if (health.PerformanceStandby == true)
        {
            return VaultHealthStatus.PerformanceStandby;
        }

        if (health.Standby == true)
        {
            return VaultHealthStatus.Standby;
        }

        if (health.Initialized == true && health.Sealed == false)
        {
            return VaultHealthStatus.Active;
        }

        return GetVaultStatus(statusCode);
    }

    private static VaultHealthStatus GetVaultStatus(HttpStatusCode statusCode)
    {
        return (int)statusCode switch
        {
            ACTIVE_STATUS_CODE => VaultHealthStatus.Active,
            STANDBY_STATUS_CODE => VaultHealthStatus.Standby,
            DISASTER_RECOVERY_SECONDARY_STATUS_CODE => VaultHealthStatus.DisasterRecoverySecondary,
            PERFORMANCE_STANDBY_STATUS_CODE => VaultHealthStatus.PerformanceStandby,
            HIGH_AVAILABILITY_UNHEALTHY_STATUS_CODE => VaultHealthStatus.HighAvailabilityUnhealthy,
            UNINITIALIZED_STATUS_CODE => VaultHealthStatus.Uninitialized,
            SEALED_STATUS_CODE => VaultHealthStatus.Sealed,
            REMOVED_STATUS_CODE => VaultHealthStatus.Removed,
            _ => VaultHealthStatus.Unknown
        };
    }

    private static Dictionary<string, object> CreateData(
        HttpResponseMessage response,
        VaultHealthResponse? health,
        VaultHealthStatus vaultStatus)
    {
        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "vaultStatus", vaultStatus.ToString() },
            { "statusCode", (int)response.StatusCode }
        };

        if (health is null)
        {
            return data;
        }

        if (health.Initialized is not null)
        {
            data["initialized"] = health.Initialized.Value;
        }

        if (health.Sealed is not null)
        {
            data["sealed"] = health.Sealed.Value;
        }

        if (health.Standby is not null)
        {
            data["standby"] = health.Standby.Value;
        }

        if (health.PerformanceStandby is not null)
        {
            data["performanceStandby"] = health.PerformanceStandby.Value;
        }

        if (!string.IsNullOrWhiteSpace(health.ReplicationDisasterRecoveryMode))
        {
            data["replicationDisasterRecoveryMode"] = health.ReplicationDisasterRecoveryMode!;
        }

        if (!string.IsNullOrWhiteSpace(health.ReplicationPerformanceMode))
        {
            data["replicationPerformanceMode"] = health.ReplicationPerformanceMode!;
        }

        if (health.HighAvailabilityConnectionHealthy is not null)
        {
            data["highAvailabilityConnectionHealthy"] = health.HighAvailabilityConnectionHealthy.Value;
        }

        if (health.RemovedFromCluster is not null)
        {
            data["removedFromCluster"] = health.RemovedFromCluster.Value;
        }

        if (!string.IsNullOrWhiteSpace(health.Version))
        {
            data["version"] = health.Version!;
        }

        if (!string.IsNullOrWhiteSpace(health.ClusterName))
        {
            data["clusterName"] = health.ClusterName!;
        }

        if (!string.IsNullOrWhiteSpace(health.ClusterId))
        {
            data["clusterId"] = health.ClusterId!;
        }

        if (health.ServerTimeUtc is not null)
        {
            data["serverTimeUtc"] = health.ServerTimeUtc.Value;
        }

        return data;
    }

    private sealed class VaultHealthResponse
    {
        [JsonPropertyName("initialized")]
        public bool? Initialized { get; set; }

        [JsonPropertyName("sealed")]
        public bool? Sealed { get; set; }

        [JsonPropertyName("standby")]
        public bool? Standby { get; set; }

        [JsonPropertyName("performance_standby")]
        public bool? PerformanceStandby { get; set; }

        [JsonPropertyName("replication_dr_mode")]
        public string? ReplicationDisasterRecoveryMode { get; set; }

        [JsonPropertyName("replication_performance_mode")]
        public string? ReplicationPerformanceMode { get; set; }

        [JsonPropertyName("ha_connection_healthy")]
        public bool? HighAvailabilityConnectionHealthy { get; set; }

        [JsonPropertyName("removed_from_cluster")]
        public bool? RemovedFromCluster { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("cluster_name")]
        public string? ClusterName { get; set; }

        [JsonPropertyName("cluster_id")]
        public string? ClusterId { get; set; }

        [JsonPropertyName("server_time_utc")]
        public long? ServerTimeUtc { get; set; }
    }
}
