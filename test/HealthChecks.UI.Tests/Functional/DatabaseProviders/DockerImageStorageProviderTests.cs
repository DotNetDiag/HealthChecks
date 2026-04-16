using HealthChecks.UI.Data;
using Microsoft.Extensions.Configuration;

namespace HealthChecks.UI.Tests;

public class docker_image_storage_provider_configuration_should
{
    private const string SqlProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string PostgreProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";
    private const string MySqlProviderName = "Pomelo.EntityFrameworkCore.MySql";

#pragma warning disable ASPDEPR004, ASPDEPR008 // WebHostBuilder/IWebHost are required by Startup-based image tests
    private static IWebHost BuildHost(IEnumerable<KeyValuePair<string, string?>>? settings = null)
    {
        return new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                if (settings is not null)
                {
                    config.AddInMemoryCollection(settings);
                }
            })
            .UseStartup<HealthChecks.UI.Image.Startup>()
            .Build();
    }
#pragma warning restore ASPDEPR004, ASPDEPR008 // WebHostBuilder/IWebHost are required by Startup-based image tests

    [Fact]
    public void fail_with_invalid_storage_provider_value()
    {
        Should.Throw<ArgumentException>(() => BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", "invalidvalue")
        ]));
    }
    [Fact]
    public void register_sql_server()
    {
        var host = BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.SqlServer.ToString()),
            new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
        ]);

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqlProviderName);
    }

    [Fact]
    public void fail_to_register_sql_server_with_no_connection_string()
    {
        Should.Throw<ArgumentNullException>(() => BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.SqlServer.ToString())
        ]));
    }

    [Fact]
    public void register_sqlite()
    {
        var host = BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.Sqlite.ToString()),
            new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
        ]);

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqliteProviderName);
    }

    [Fact]
    public void fail_to_register_sqlite_with_no_connection_string()
    {
        Should.Throw<ArgumentNullException>(() => BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.Sqlite.ToString())
        ]));
    }

    [Fact]
    public void register_postgresql()
    {
        var host = BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.PostgreSql.ToString()),
            new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
        ]);

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(PostgreProviderName);
    }

    [Fact]
    public void fail_to_register_postgresql_with_no_connection_string()
    {
        Should.Throw<ArgumentNullException>(() => BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.PostgreSql.ToString())
        ]));
    }

    [Fact]
    public void register_inmemory()
    {
        var host = BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.InMemory.ToString())
        ]);

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_inmemory_as_default_provider_when_no_option_is_configured()
    {
        var host = BuildHost();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_mysql()
    {
        var host = BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.MySql.ToString()),
            new KeyValuePair<string, string?>("storage_connection", "Host=localhost;User Id=root;Password=Password12!;Database=UI"),
        ]);

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(MySqlProviderName);
    }

    [Fact]
    public void fail_to_register_mysql_with_no_connection_string()
    {
        Should.Throw<ArgumentNullException>(() => BuildHost(
        [
            new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.MySql.ToString())
        ]));
    }

}
