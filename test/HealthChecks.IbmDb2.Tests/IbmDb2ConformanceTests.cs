using System.Net;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.IbmDb2.Tests;

public class IbmDb2ConformanceTests
{
    [Fact]
    public void HealthCheckTypeIsSealed() => Assert.True(typeof(IbmDb2HealthCheck).IsSealed);

    [Fact]
    public void OptionsTypeIsSealed() => Assert.True(typeof(IbmDb2HealthCheckOptions).IsSealed);

    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullOptions()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new IbmDb2HealthCheck(options: null!));

        Assert.False(string.IsNullOrEmpty(argumentNullException.ParamName));
    }

    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullConnectionString()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new IbmDb2HealthCheck(new IbmDb2HealthCheckOptions()));

        Assert.False(string.IsNullOrEmpty(argumentNullException.ParamName));
    }

    [LinuxFact]
    public async Task ReturnsProvidedFailureStatusWhenConnectionCanNotBeMade()
    {
        foreach (HealthStatus failureStatus in new[] { HealthStatus.Unhealthy, HealthStatus.Degraded })
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            builder.Services.AddHealthChecks()
                .AddIbmDb2(CreateConnectionStringForNonExistingEndpoint(), failureStatus: failureStatus);

            await using WebApplication app = builder.Build();
            app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });
            await app.StartAsync();
            IHost host = app;

            using HttpResponseMessage response = await host.GetTestClient().GetAsync("/health");

            response.StatusCode.ShouldBe(failureStatus == HealthStatus.Unhealthy ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        }
    }

    [Fact]
    public void DependencyInjectionRegistrationWorksAsExpected()
    {
        const string healthCheckName = "random_name";
        var timeout = TimeSpan.FromSeconds(5);
        string[] tags = ["a", "b", "c"];
        int counter = 0;

        var services = new ServiceCollection();

        services.AddSingleton(_ =>
        {
            counter++;
            return CreateConnectionStringForNonExistingEndpoint();
        });

        services.AddHealthChecks()
            .AddIbmDb2(
                sp => sp.GetRequiredService<string>(),
                name: healthCheckName,
                failureStatus: HealthStatus.Degraded,
                tags: tags,
                timeout: timeout);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.Single();

        counter.ShouldBe(0);

        IHealthCheck check = registration.Factory(serviceProvider);
        check.ShouldBeOfType<IbmDb2HealthCheck>();

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

    private static string CreateConnectionStringForNonExistingEndpoint()
        => "Server=127.0.0.1:1;Database=testdb;UID=db2inst1;PWD=password;Connect Timeout=1;";
}
