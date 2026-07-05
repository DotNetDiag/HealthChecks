using System.Net;
using System.Net.Http.Headers;
using HealthChecks.Harbor.Tests.Helpers;

namespace HealthChecks.Harbor.Tests;

public class harborhealthcheck_should
{
    private const string HealthCheckName = "unit-test-check";
    private const string HealthyResponse = """
        {
          "status": "healthy",
          "components": [
            { "name": "core", "status": "healthy" },
            { "name": "portal", "status": "healthy" },
            { "name": "jobservice", "status": "healthy" },
            { "name": "registry", "status": "healthy" },
            { "name": "database", "status": "healthy" }
          ]
        }
        """;

    [Fact]
    public async Task call_expected_harbor_health_endpoint()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://harbor.example.com/api/v2.0/health");
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
        actual.Data["component:registry"].ShouldBe("healthy");
    }

    [Fact]
    public async Task use_custom_health_endpoint_path_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client, new HarborHealthCheckOptions
        {
            BaseUri = new Uri("https://proxy.example.com"),
            HealthEndpointPath = "/harbor/api/v2.0/health"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://proxy.example.com/harbor/api/v2.0/health");
    }

    [Fact]
    public async Task invoke_configure_request_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client, new HarborHealthCheckOptions
        {
            ConfigureRequest = request => request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "dXNlcjpwYXNz")
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.AuthorizationHeader.ShouldBe("Basic dXNlcjpwYXNz");
    }

    [Fact]
    public async Task return_failure_status_when_endpoint_is_not_successful()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Harbor health endpoint returned HTTP 503 (Service Unavailable).");
    }

    [Fact]
    public async Task return_failure_status_when_overall_status_is_not_healthy()
    {
        const string response = """
            {
              "status": "unhealthy",
              "components": [
                { "name": "database", "status": "unhealthy" },
                { "name": "registry", "status": "healthy" }
              ]
            }
            """;
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, response);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Harbor reported status 'unhealthy'. Unhealthy components: database=unhealthy.");
        actual.Data["component:database"].ShouldBe("unhealthy");
    }

    [Fact]
    public async Task return_failure_status_when_component_is_not_healthy()
    {
        const string response = """
            {
              "status": "healthy",
              "components": [
                { "name": "jobservice", "status": "unhealthy" }
              ]
            }
            """;
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, response);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Harbor component status is not healthy: jobservice=unhealthy.");
    }

    [Fact]
    public async Task return_failure_status_when_required_component_is_missing()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, HealthyResponse);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client, new HarborHealthCheckOptions
        {
            RequiredComponents =
            {
                "registry",
                "redis"
            }
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Harbor health response did not include required components: redis.");
    }

    [Fact]
    public async Task return_failure_status_when_health_response_has_no_status()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"components":[]}""");
        using HttpClient client = CreateClient(handler);
        var healthCheck = new HarborHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Harbor health response did not include a status value.");
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK, """{"status":"unhealthy"}""");
        using HttpClient client = CreateClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddHarbor(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("Harbor reported status 'unhealthy'.");
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://harbor.example.com")
        };

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
