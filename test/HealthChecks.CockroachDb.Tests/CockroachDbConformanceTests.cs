using System.Net;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.CockroachDb.Tests;

public class CockroachDbConformanceTests
{
    [Fact]
    public void health_check_type_is_sealed() => typeof(CockroachDbHealthCheck).IsSealed.ShouldBeTrue();

    [Fact]
    public void options_type_is_sealed() => typeof(CockroachDbHealthCheckOptions).IsSealed.ShouldBeTrue();

    [Fact]
    public void ctor_throws_argument_null_exception_for_null_options()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new CockroachDbHealthCheck(options: null!));

        argumentNullException.ParamName.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(HealthStatus.Unhealthy, true)]
    [InlineData(HealthStatus.Unhealthy, false)]
    [InlineData(HealthStatus.Degraded, true)]
    [InlineData(HealthStatus.Degraded, false)]
    public async Task returns_provided_failure_status_when_connection_can_not_be_made(HealthStatus failureStatus, bool useDiExtension)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        var options = new CockroachDbHealthCheckOptions("Host=127.0.0.1;Port=1;Username=root;Database=defaultdb;SSL Mode=Disable");

        if (useDiExtension)
        {
            builder.Services.AddHealthChecks()
                .AddCockroachDb(options, failureStatus: failureStatus, timeout: TimeSpan.FromSeconds(3));
        }
        else
        {
            builder.Services.AddHealthChecks()
                .Add(new HealthCheckRegistration(
                    name: "name",
                    instance: new CockroachDbHealthCheck(options),
                    failureStatus: failureStatus,
                    tags: null,
                    timeout: TimeSpan.FromSeconds(3)));
        }

        await using var app = builder.Build();
        app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });
        await app.StartAsync();
        var host = (IHost)app;

        using var response = await host.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(failureStatus == HealthStatus.Unhealthy ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
    }

    [Fact]
    public void dependency_injection_registration_works_as_expected()
    {
        const string healthCheckName = "random_name";
        var timeout = TimeSpan.FromSeconds(5);
        string[] tags = ["a", "b", "c"];
        int counter = 0;

        ServiceCollection services = new();
        services.AddHealthChecks()
            .AddCockroachDb(
                _ =>
                {
                    counter++;
                    return new CockroachDbHealthCheckOptions("Host=127.0.0.1;Port=1;Username=root;Database=defaultdb;SSL Mode=Disable");
                },
                healthCheckName,
                HealthStatus.Degraded,
                tags,
                timeout);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.Single();

        counter.ShouldBe(0);

        IHealthCheck check = registration.Factory(serviceProvider);
        check.ShouldBeOfType<CockroachDbHealthCheck>();

        counter.ShouldBe(1);

        registration.Name.ShouldBe(healthCheckName);
        registration.FailureStatus.ShouldBe(HealthStatus.Degraded);
        registration.Tags.ToArray().ShouldBeEquivalentTo(tags);
        registration.Timeout.ShouldBe(timeout);

        for (int i = 0; i < 10; i++)
        {
            registration.Factory(serviceProvider);
        }

        counter.ShouldBe(1);
    }

    [Fact]
    public async Task returns_unhealthy_when_node_health_endpoint_fails()
    {
        var httpClient = new HttpClient(new RecordingHttpMessageHandler(HttpStatusCode.ServiceUnavailable));
        var healthCheck = new CockroachDbHealthCheck(
            new CockroachDbHealthCheckOptions(new Uri("http://localhost:8080/health?ready=1")),
            httpClient);

        var registration = new HealthCheckRegistration("cockroachdb", healthCheck, HealthStatus.Unhealthy, tags: null);
        var context = new HealthCheckContext { Registration = registration };

        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldNotBeNull();
        result.Description.ShouldContain("503");
        result.Data["statusCode"].ShouldBe(503);
    }

    private sealed class RecordingHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
