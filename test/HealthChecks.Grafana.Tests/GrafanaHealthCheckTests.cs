using System.Net;
using System.Net.Http.Headers;
using HealthChecks.Grafana.Tests.Helpers;

namespace HealthChecks.Grafana.Tests;

public class grafanahealthcheck_should
{
    private const string HealthCheckName = "unit-test-check";
    private const string HealthyResponse = """
        {
          "commit": "8bb8739",
          "database": "ok",
          "version": "12.1.0"
        }
        """;

    [Fact]
    public async Task call_expected_grafana_health_endpoint()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://grafana.example.com/api/health");
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
        actual.Data["database"].ShouldBe("ok");
        actual.Data["version"].ShouldBe("12.1.0");
        actual.Data["commit"].ShouldBe("8bb8739");
    }

    [Fact]
    public async Task use_custom_health_endpoint_path_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client, new GrafanaHealthCheckOptions
        {
            BaseUri = new Uri("https://proxy.example.com"),
            HealthEndpointPath = "/grafana/api/health"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://proxy.example.com/grafana/api/health");
    }

    [Fact]
    public async Task invoke_configure_request_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client, new GrafanaHealthCheckOptions
        {
            ConfigureRequest = request => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token")
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.AuthorizationHeader.ShouldBe("Bearer token");
    }

    [Fact]
    public async Task return_failure_status_when_endpoint_is_not_successful()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Grafana health endpoint returned HTTP 503 (Service Unavailable).");
    }

    [Fact]
    public async Task return_failure_status_when_database_status_is_not_ok()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"database":"failing"}""");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Grafana reported database status 'failing'.");
        actual.Data["database"].ShouldBe("failing");
    }

    [Fact]
    public async Task return_failure_status_when_health_response_has_no_database_status()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"version":"12.1.0"}""");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new GrafanaHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Grafana health response did not include a database status value.");
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"database":"failing"}""");
        using HttpClient client = CreateClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddGrafana(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("Grafana reported database status 'failing'.");
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://grafana.example.com")
        };

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
