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

    [Fact]
    public void fail_with_invalid_storage_provider_value()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", "invalidvalue")
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        Should.Throw<ArgumentException>(() => hostBuilder.Build());
    }
    [Fact]
    public void register_sql_server()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.SqlServer.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqlProviderName);
    }

    [Fact]
    public void fail_to_register_sql_server_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.SqlServer.ToString())
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_sqlite()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.Sqlite.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqliteProviderName);
    }

    [Fact]
    public void fail_to_register_sqlite_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.Sqlite.ToString())
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_postgresql()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.PostgreSql.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(PostgreProviderName);
    }

    [Fact]
    public void fail_to_register_postgresql_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.PostgreSql.ToString())
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_inmemory()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.InMemory.ToString())
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_inmemory_as_default_provider_when_no_option_is_configured()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config => config.Sources.Clear())
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_mysql()
    {
        //
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.MySql.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "Host=localhost;User Id=root;Password=Password12!;Database=UI"),

                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(MySqlProviderName);
    }

    [Fact]
    public void fail_to_register_mysql_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", HealthChecks.UI.Image.Configuration.StorageProviderEnum.MySql.ToString())
                });
            })
            .UseStartup<HealthChecks.UI.Image.Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

}
