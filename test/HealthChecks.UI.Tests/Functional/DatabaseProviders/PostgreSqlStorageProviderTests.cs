using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Tests;

[Collection("execution")]
public class postgre_storage_should
{
    private const string ProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

    [Fact]
    public void register_healthchecksdb_context_with_migrations()
    {
        var customOptionsInvoked = false;

        using var host = TestHostHelper.Build(startHost: false, webHostBuilder => webHostBuilder
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecksUI()
                .AddPostgreSqlStorage("connectionString", options => customOptionsInvoked = true);
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
        await ProviderTestHelper.WaitForPostgresAsync();

        var hostReset = new ManualResetEventSlim(false);
        var collectorReset = new ManualResetEventSlim(false);

        using var appHost = HostBuilderHelper.Create(
            hostReset,
            collectorReset,
            configureUI: config => config.AddPostgreSqlStorage(ProviderTestHelper.PostgresConnectionString()));

        var server = appHost.GetTestServer();

        ProviderTestHelper.WaitForHost(hostReset);

        var context = appHost.Services.GetRequiredService<HealthChecksDb>();
        var configurations = await context.Configurations.ToListAsync();
        var host1 = ProviderTestHelper.Endpoints[0];

        configurations[0].Name.ShouldBe(host1.Name);
        configurations[0].Uri.ShouldBe(host1.Uri);

        using var client = server.CreateClient();

        ProviderTestHelper.WaitForCollector(collectorReset);

        var execution = await ProviderTestHelper.WaitForExecutionAsync(client, host1.Name);
        execution.Name.ShouldBe(host1.Name);
    }
}
