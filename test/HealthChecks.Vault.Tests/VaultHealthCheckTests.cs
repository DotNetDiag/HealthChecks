using System.Net;
using System.Net.Http.Headers;
using HealthChecks.Vault.Tests.Helpers;

namespace HealthChecks.Vault.Tests;

public class vaulthealthcheck_should
{
    private const string HealthCheckName = "unit-test-check";
    private const string ActiveResponse = """
        {
          "initialized": true,
          "sealed": false,
          "standby": false,
          "performance_standby": false,
          "replication_dr_mode": "disabled",
          "replication_performance_mode": "disabled",
          "server_time_utc": 1710000000,
          "version": "1.17.0",
          "cluster_name": "vault-cluster",
          "cluster_id": "vault-cluster-id"
        }
        """;
    private const string StandbyResponse = """{"initialized":true,"sealed":false,"standby":true}""";
    private const string PerformanceStandbyResponse = """{"initialized":true,"sealed":false,"performance_standby":true}""";
    private const string DisasterRecoverySecondaryResponse = """{"initialized":true,"sealed":false,"replication_dr_mode":"secondary"}""";
    private const string SealedResponse = """{"initialized":true,"sealed":true}""";
    private const string UninitializedResponse = """{"initialized":false,"sealed":true}""";
    private const string HighAvailabilityUnhealthyResponse = """{"initialized":true,"sealed":false,"standby":true,"ha_connection_healthy":false}""";
    private const string RemovedResponse = """{"initialized":true,"sealed":false,"removed_from_cluster":true}""";

    public static TheoryData<HttpStatusCode, string, VaultHealthStatus> NonHealthyVaultStates()
        => new()
        {
            { HttpStatusCode.TooManyRequests, StandbyResponse, VaultHealthStatus.Standby },
            { (HttpStatusCode)473, PerformanceStandbyResponse, VaultHealthStatus.PerformanceStandby },
            { (HttpStatusCode)472, DisasterRecoverySecondaryResponse, VaultHealthStatus.DisasterRecoverySecondary },
            { HttpStatusCode.ServiceUnavailable, SealedResponse, VaultHealthStatus.Sealed },
            { HttpStatusCode.NotImplemented, UninitializedResponse, VaultHealthStatus.Uninitialized },
            { (HttpStatusCode)474, HighAvailabilityUnhealthyResponse, VaultHealthStatus.HighAvailabilityUnhealthy },
            { (HttpStatusCode)530, RemovedResponse, VaultHealthStatus.Removed },
        };

    [Fact]
    public async Task call_expected_vault_health_endpoint()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, ActiveResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://vault.example.com/v1/sys/health");
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
        actual.Data["vaultStatus"].ShouldBe("Active");
        actual.Data["statusCode"].ShouldBe(200);
        actual.Data["initialized"].ShouldBe(true);
        actual.Data["sealed"].ShouldBe(false);
        actual.Data["standby"].ShouldBe(false);
        actual.Data["performanceStandby"].ShouldBe(false);
        actual.Data["replicationDisasterRecoveryMode"].ShouldBe("disabled");
        actual.Data["replicationPerformanceMode"].ShouldBe("disabled");
        actual.Data["serverTimeUtc"].ShouldBe(1710000000L);
        actual.Data["version"].ShouldBe("1.17.0");
        actual.Data["clusterName"].ShouldBe("vault-cluster");
        actual.Data["clusterId"].ShouldBe("vault-cluster-id");
    }

    [Fact]
    public async Task use_custom_health_endpoint_path_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, ActiveResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client, new VaultHealthCheckOptions
        {
            BaseUri = new Uri("https://proxy.example.com"),
            HealthEndpointPath = "/vault/v1/sys/health"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://proxy.example.com/vault/v1/sys/health");
    }

    [Fact]
    public async Task use_absolute_health_endpoint_uri_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, ActiveResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client, new VaultHealthCheckOptions
        {
            HealthEndpointPath = "https://probe.example.com/v1/sys/health"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://probe.example.com/v1/sys/health");
    }

    [Fact]
    public async Task invoke_configure_request_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, ActiveResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client, new VaultHealthCheckOptions
        {
            ConfigureRequest = request => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "vault-token")
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.AuthorizationHeader.ShouldBe("Bearer vault-token");
    }

    [Theory]
    [MemberData(nameof(NonHealthyVaultStates))]
    public async Task return_failure_status_when_vault_reports_non_healthy_status(
        HttpStatusCode statusCode,
        string responseContent,
        VaultHealthStatus expectedVaultStatus)
    {
        var handler = new RecordingHttpMessageHandler(statusCode, responseContent);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe($"Vault reported status '{expectedVaultStatus}' with HTTP {(int)statusCode}.");
        actual.Data["vaultStatus"].ShouldBe(expectedVaultStatus.ToString());
        actual.Data["statusCode"].ShouldBe((int)statusCode);
    }

    [Fact]
    public async Task identify_standby_from_response_body_when_standby_ok_returns_success()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, StandbyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Vault reported status 'Standby' with HTTP 200.");
        actual.Data["vaultStatus"].ShouldBe("Standby");
    }

    [Fact]
    public async Task return_healthy_for_configured_healthy_statuses()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.TooManyRequests, StandbyResponse);
        using HttpClient client = CreateClient(handler);
        var options = new VaultHealthCheckOptions();
        options.HealthyStatuses.Add(VaultHealthStatus.Standby);
        var healthCheck = new VaultHealthCheck(client, options);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        actual.Data["vaultStatus"].ShouldBe("Standby");
    }

    [Fact]
    public async Task return_failure_status_when_response_is_invalid_json()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, "{");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Vault health endpoint returned invalid JSON.");
    }

    [Fact]
    public async Task return_failure_status_when_status_cannot_be_identified()
    {
        var handler = new RecordingHttpMessageHandler((HttpStatusCode)418, string.Empty);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new VaultHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Vault health endpoint returned HTTP 418 without a recognizable Vault health status.");
        actual.Data["vaultStatus"].ShouldBe("Unknown");
        actual.Data["statusCode"].ShouldBe(418);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.TooManyRequests, StandbyResponse);
        using HttpClient client = CreateClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddVault(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("Vault reported status 'Standby' with HTTP 429.");
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://vault.example.com")
        };

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
