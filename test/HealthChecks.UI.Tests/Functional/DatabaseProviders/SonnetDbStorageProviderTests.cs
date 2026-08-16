using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Tests;

[Collection("execution")]
public class sonnetdb_storage_should
{
    private const string ProviderName = "SonnetDB.EntityFrameworkCore";

    [Fact]
    public void register_healthchecksdb_context_with_migrations()
    {
        var customOptionsInvoked = false;

        using var host = TestHostHelper.Build(startHost: false, webHostBuilder => webHostBuilder
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecksUI()
                    .AddSonnetDBStorage("Data Source=./healthchecks-ui-test", options => customOptionsInvoked = true);
            }));

        var services = host.Services;
        var context = services.GetRequiredService<HealthChecksDb>();

        context.ShouldNotBeNull();
        context.Database.GetMigrations().Count().ShouldBeGreaterThan(0);
        context.Database.ProviderName.ShouldBe(ProviderName);
        customOptionsInvoked.ShouldBeTrue();
    }
}
