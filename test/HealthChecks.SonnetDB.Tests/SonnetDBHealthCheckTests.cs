using System.Net;
using System.Net.Http.Headers;
using HealthChecks.SonnetDB.Tests.Helpers;

namespace HealthChecks.SonnetDB.Tests;

public class sonnetdbhealthcheck_should
{
    private const string HealthCheckName = "unit-test-check";
    private const string HealthyResponse = """
        {
          "status": "ok",
          "databases": 2,
          "uptimeSeconds": 42.5,
          "copilotEnabled": true,
          "copilotReady": true
        }
        """;

    [Fact]
    public async Task call_expected_sonnetdb_health_endpoint()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://sonnetdb.example.com/healthz");
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
        actual.Data["status"].ShouldBe("ok");
        actual.Data["databases"].ShouldBe(2);
        actual.Data["uptimeSeconds"].ShouldBe(42.5);
        actual.Data["copilotEnabled"].ShouldBe(true);
        actual.Data["copilotReady"].ShouldBe(true);
    }

    [Fact]
    public async Task use_custom_health_endpoint_path_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client, new SonnetDBHealthCheckOptions
        {
            BaseUri = new Uri("https://proxy.example.com"),
            HealthEndpointPath = "/sonnetdb/healthz"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://proxy.example.com/sonnetdb/healthz");
    }

    [Fact]
    public async Task use_absolute_health_endpoint_uri_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client, new SonnetDBHealthCheckOptions
        {
            HealthEndpointPath = "https://probe.example.com/healthz"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://probe.example.com/healthz");
    }

    [Fact]
    public async Task invoke_configure_request_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client, new SonnetDBHealthCheckOptions
        {
            ConfigureRequest = request => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "sonnetdb-token")
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.AuthorizationHeader.ShouldBe("Bearer sonnetdb-token");
    }

    [Fact]
    public async Task return_failure_status_when_endpoint_is_not_successful()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("SonnetDB health endpoint returned HTTP 503 (Service Unavailable).");
    }

    [Fact]
    public async Task return_failure_status_when_status_is_not_ok()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"status":"degraded"}""");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("SonnetDB reported status 'degraded'.");
        actual.Data["status"].ShouldBe("degraded");
    }

    [Fact]
    public async Task return_failure_status_when_health_response_has_no_status()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"databases":1}""");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("SonnetDB health response did not include a status value.");
    }

    [Fact]
    public async Task return_failure_status_when_required_copilot_is_not_ready()
    {
        const string response = """
            {
              "status": "ok",
              "copilotEnabled": true,
              "copilotReady": false
            }
            """;
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, response);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new SonnetDBHealthCheck(client, new SonnetDBHealthCheckOptions
        {
            RequireCopilotReady = true
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("SonnetDB Copilot is enabled but not ready.");
        actual.Data["copilotEnabled"].ShouldBe(true);
        actual.Data["copilotReady"].ShouldBe(false);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"status":"degraded"}""");
        using HttpClient client = CreateClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddSonnetDB(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("SonnetDB reported status 'degraded'.");
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://sonnetdb.example.com")
        };

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
