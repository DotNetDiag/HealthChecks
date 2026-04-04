using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Tests;

[Collection("execution")]
public class inmemory_storage_should
{
    private const string ProviderName = "Microsoft.EntityFrameworkCore.InMemory";

    [Fact]
    public void register_healthchecksdb_context()
    {
        var customOptionsInvoked = false;

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecksUI()
                .AddInMemoryStorage(opt => customOptionsInvoked = true);
            }));

        var services = host.Services;
        var context = services.GetRequiredService<HealthChecksDb>();

        context.ShouldNotBeNull();
        context.Database.ProviderName.ShouldBe(ProviderName);
        customOptionsInvoked.ShouldBeTrue();
    }

    [Fact(Skip = "conflicts with other tests that use inmemory storage too")]
    public async Task seed_database_and_serve_stored_executions()
    {
        var hostReset = new ManualResetEventSlim(false);
        var collectorReset = new ManualResetEventSlim(false);

        using var appHost = HostBuilderHelper.Create(
            hostReset,
            collectorReset,
            configureUI: config => config.AddInMemoryStorage());

        var server = appHost.GetTestServer();

        hostReset.Wait(ProviderTestHelper.DefaultHostTimeout);

        var context = appHost.Services.GetRequiredService<HealthChecksDb>();
        var configurations = await context.Configurations.ToListAsync();
        var host1 = ProviderTestHelper.Endpoints[0];

        configurations[0].Name.ShouldBe(host1.Name);
        configurations[0].Uri.ShouldBe(host1.Uri);

        using var client = server.CreateClient();

        collectorReset.Wait(ProviderTestHelper.DefaultCollectorTimeout);

        var report = await client.GetAsJson<List<HealthCheckExecution>>("/healthchecks-api");
        report.First().Name.ShouldBe(host1.Name);
    }
}
