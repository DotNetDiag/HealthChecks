using System.Net;
using System.Net.Http.Headers;
using HealthChecks.ContainerRegistry.Tests.Helpers;

namespace HealthChecks.ContainerRegistry.Tests;

public class containerregistryhealthcheck_should
{
    private const string HealthCheckName = "unit-test-check";

    [Fact]
    public async Task call_expected_registry_v2_endpoint()
    {
        var handler = new RecordingHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Docker-Distribution-API-Version", "registry/2.0");

            return response;
        });
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://registry.example.com/v2/");
        handler.RequestMethod.ShouldBe(HttpMethod.Get);
        actual.Data["statusCode"].ShouldBe(200);
        actual.Data["distributionApiVersion"].ShouldBe("registry/2.0");
    }

    [Fact]
    public async Task return_healthy_when_private_registry_returns_authentication_challenge()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "realm=\"https://auth.example.com/token\",service=\"registry.example.com\""));

            return response;
        });
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        actual.Data["authenticationChallenge"].ShouldBe(true);
        actual.Data["wwwAuthenticate"].ShouldBe("Bearer realm=\"https://auth.example.com/token\",service=\"registry.example.com\"");
    }

    [Fact]
    public async Task use_custom_registry_endpoint_path_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client, new ContainerRegistryHealthCheckOptions
        {
            BaseUri = new Uri("https://proxy.example.com"),
            RegistryEndpointPath = "/registry/v2/"
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        handler.RequestUri.ShouldNotBeNull();
        handler.RequestUri.AbsoluteUri.ShouldBe("https://proxy.example.com/registry/v2/");
    }

    [Fact]
    public async Task invoke_configure_request_when_configured()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client, new ContainerRegistryHealthCheckOptions
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
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Container registry endpoint returned HTTP 503 (Service Unavailable).");
        actual.Data["statusCode"].ShouldBe(503);
    }

    [Fact]
    public async Task return_failure_status_when_unauthorized_response_has_no_authentication_challenge()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.Unauthorized);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client);

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Container registry endpoint returned HTTP 401 (Unauthorized) without a WWW-Authenticate challenge.");
    }

    [Fact]
    public async Task return_failure_status_when_unauthorized_response_is_not_allowed()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "realm=\"registry\""));

            return response;
        });
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client, new ContainerRegistryHealthCheckOptions
        {
            AllowUnauthorizedResponse = false
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck, HealthStatus.Degraded));

        actual.Status.ShouldBe(HealthStatus.Degraded);
        actual.Description.ShouldBe("Container registry endpoint returned HTTP 401 (Unauthorized).");
    }

    [Fact]
    public async Task return_healthy_when_unauthorized_response_has_no_challenge_but_challenge_requirement_is_disabled()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.Unauthorized);
        using HttpClient client = CreateClient(handler);
        var healthCheck = new ContainerRegistryHealthCheck(client, new ContainerRegistryHealthCheckOptions
        {
            RequireAuthenticationChallenge = false
        });

        HealthCheckResult actual = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        actual.Status.ShouldBe(HealthStatus.Healthy);
        actual.Data["authenticationChallenge"].ShouldBe(false);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.Forbidden);
        using HttpClient client = CreateClient(handler);
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(client)
            .AddLogging()
            .AddHealthChecks()
            .AddContainerRegistry(name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Description.ShouldBe("Container registry endpoint returned HTTP 403 (Forbidden).");
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
        => new(handler)
        {
            BaseAddress = new Uri("https://registry.example.com")
        };

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, failureStatus, null)
        };
    }
}
