using System.Net;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.DuckDb.Tests;

public class DuckDbConformanceTests
{
    [Fact]
    public void health_check_type_is_sealed() => typeof(DuckDbHealthCheck).IsSealed.ShouldBeTrue();

    [Fact]
    public void options_type_is_sealed() => typeof(DuckDbHealthCheckOptions).IsSealed.ShouldBeTrue();

    [Fact]
    public void ctor_throws_argument_null_exception_for_null_options()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new DuckDbHealthCheck(options: null!));

        argumentNullException.ParamName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ctor_throws_argument_null_exception_for_null_connection_string()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new DuckDbHealthCheck(new DuckDbHealthCheckOptions()));

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

        var options = new DuckDbHealthCheckOptions(CreateConnectionStringForNonExistingDatabase());

        if (useDiExtension)
        {
            builder.Services.AddHealthChecks()
                .AddDuckDb(options, failureStatus: failureStatus);
        }
        else
        {
            builder.Services.AddHealthChecks()
                .Add(new HealthCheckRegistration(
                    name: "name",
                    instance: new DuckDbHealthCheck(options),
                    failureStatus: failureStatus,
                    tags: null));
        }

        await using WebApplication app = builder.Build();
        app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });
        await app.StartAsync();
        IHost host = app;

        using HttpResponseMessage response = await host.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(failureStatus == HealthStatus.Unhealthy ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
    }

    [Fact]
    public void dependency_injection_registration_works_as_expected()
    {
        const string healthCheckName = "random_name";
        var timeout = TimeSpan.FromSeconds(5);
        string[] tags = ["a", "b", "c"];
        int counter = 0;

        var services = new ServiceCollection();

        services.AddHealthChecks()
            .AddDuckDb(
                _ =>
                {
                    counter++;

                    return new DuckDbHealthCheckOptions(CreateConnectionStringForNonExistingDatabase());
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
        check.ShouldBeOfType<DuckDbHealthCheck>();

        counter.ShouldBe(1);

        registration.Name.ShouldBe(healthCheckName);
        registration.FailureStatus.ShouldBe(HealthStatus.Degraded);
        registration.Tags.ToArray().ShouldBeEquivalentTo(tags);
        registration.Timeout.ShouldBe(timeout);
    }

    [Fact]
    public async Task custom_result_builder_can_shape_the_result()
    {
        var healthCheck = new DuckDbHealthCheck(new DuckDbHealthCheckOptions("Data Source=:memory:")
        {
            HealthCheckResultBuilder = result => HealthCheckResult.Degraded($"DuckDB returned {result}")
        });

        var registration = new HealthCheckRegistration("duckdb", healthCheck, HealthStatus.Unhealthy, tags: null);
        var context = new HealthCheckContext { Registration = registration };

        HealthCheckResult result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldBe("DuckDB returned 1");
    }

    internal static string CreateConnectionStringForNonExistingDatabase()
        => "Data Source=/path/that/does/not/exist/healthchecks.duckdb";
}
