using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Tests;

public class mysql_storage_should
{
#if NET8_0
    private const string PROVIDER_NAME = "Pomelo.EntityFrameworkCore.MySql";
#else
    private const string PROVIDER_NAME = "Microting.EntityFrameworkCore.MySql";
#endif

    [Fact]
    public void register_healthchecksdb_context_with_migrations()
    {
        var customOptionsInvoked = false;

        using var host = TestHostHelper.Build(startHost: false, webHostBuilder => webHostBuilder
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecksUI()
                .AddMySqlStorage("Host=localhost;User Id=root;Password=Password12!;Database=UI", options => customOptionsInvoked = true);
            }));

        var services = host.Services;
        var context = services.GetRequiredService<HealthChecksDb>();

        context.ShouldNotBeNull();
        context.Database.GetMigrations().Count().ShouldBeGreaterThan(0);
        context.Database.ProviderName.ShouldBe(PROVIDER_NAME);
        customOptionsInvoked.ShouldBeTrue();
    }

#if NET10_0
    [Fact]
    public void target_microting_mysql_provider_assembly()
    {
        var referencedAssemblies = typeof(HealthChecks.UI.MySql.Storage.Migrations.Initial).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        referencedAssemblies.ShouldContain("Microting.EntityFrameworkCore.MySql");
        referencedAssemblies.ShouldNotContain("Pomelo.EntityFrameworkCore.MySql");
    }
#endif

    [Fact]
    public async Task seed_database_and_serve_stored_executions()
    {
        await ProviderTestHelper.WaitForMySqlAsync();

        var hostReset = new ManualResetEventSlim(false);
        var collectorReset = new ManualResetEventSlim(false);

        using var appHost = HostBuilderHelper.Create(
            hostReset,
            collectorReset,
            configureUI: config => config.AddMySqlStorage(ProviderTestHelper.MySqlConnectionString()));

        var server = appHost.GetTestServer();

        ProviderTestHelper.WaitForHost(hostReset);

        var context = appHost.Services.GetRequiredService<HealthChecksDb>();
        var configurations = await context.Configurations.ToListAsync();
        var host1 = ProviderTestHelper.Endpoints[0];

        configurations[0].Name.ShouldBe(host1.Name);
        configurations[0].Uri.ShouldBe(host1.Uri);

        using var client = server.CreateClient();

        ProviderTestHelper.WaitForCollector(collectorReset);

        var report = await client.GetAsJson<List<HealthCheckExecution>>("/healthchecks-api");
        report.First().Name.ShouldBe(host1.Name);
    }
}
