namespace HealthChecks.SonnetDB.Tests.Functional;

public class sonnetdb_healthcheck_should(SonnetDBContainerFixture sonnetDBContainerFixture) : IClassFixture<SonnetDBContainerFixture>
{
    [Fact]
    public async Task report_healthy_for_latest_sonnetdb_image()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddHealthChecks()
            .AddSonnetDB(sonnetDBContainerFixture.GetBaseUri(), tags: ["sonnetdb"])
            .Services
            .BuildServiceProvider();

        HealthCheckService service = provider.GetRequiredService<HealthCheckService>();
        HealthReport report = await service.CheckHealthAsync();

        HealthReportEntry actual = report.Entries["sonnetdb"];
        actual.Status.ShouldBe(HealthStatus.Healthy);
        actual.Data["status"].ShouldBe("ok");
        actual.Data.ContainsKey("databases").ShouldBeTrue();
        actual.Data.ContainsKey("uptimeSeconds").ShouldBeTrue();
        actual.Data.ContainsKey("copilotEnabled").ShouldBeTrue();
        actual.Data.ContainsKey("copilotReady").ShouldBeTrue();
    }
}
