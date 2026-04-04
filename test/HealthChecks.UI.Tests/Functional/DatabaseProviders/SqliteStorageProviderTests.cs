using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Tests;

public class sqlite_storage_should
{
    private const string ProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    [Fact]
    public void register_healthchecksdb_context_with_migrations()
    {
        var customOptionsInvoked = false;

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecksUI()
                .AddSqliteStorage("connectionString", options => customOptionsInvoked = true);
            }));

        var services = host.Services;
        var context = services.GetRequiredService<HealthChecksDb>();

        context.ShouldNotBeNull();
        context.Database.GetMigrations().Count().ShouldBeGreaterThan(0);
        context.Database.ProviderName.ShouldBe(ProviderName);
        customOptionsInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task seed_database_and_serve_stored_executions()
    {
        var hostReset = new ManualResetEventSlim(false);
        var collectorReset = new ManualResetEventSlim(false);

        using var appHost = HostBuilderHelper.Create(
            hostReset,
            collectorReset,
            configureUI: setup => setup.AddSqliteStorage(ProviderTestHelper.SqliteConnectionString()));

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
