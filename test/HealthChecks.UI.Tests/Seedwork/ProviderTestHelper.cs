using System.Diagnostics;
using HealthChecks.UI.Data;

namespace HealthChecks.UI.Tests;

public class ProviderTestHelper
{
    // Used to populate IServerAddressesFeature for TestServer; no socket is bound.
    public const string TestServerAddress = "http://localhost:5000";
    public const int DefaultHostTimeout = 1000;
    public const int DefaultCollectorTimeout = 15000;

    public static List<(string Name, string Uri)> Endpoints = new()
    {
        ("host1", "/health"),
        ("host2", "/health")
    };

    public static string SqlServerConnectionString() => "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!;TrustServerCertificate=true";
    public static string PostgresConnectionString() => "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=ui";
    public static string PostgresServerConnectionString() => "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;";
    public static string MySqlConnectionString() => "Host=localhost;User Id=root;Password=Password12!;Database=UI";
    public static string MySqlServerConnectionString() => "Host=localhost;User Id=root;Password=Password12!;";
    public static string SqliteConnectionString() => "Data Source = sqlite.db";

    public static void WaitForHost(ManualResetEventSlim resetEvent)
    {
        resetEvent.Wait(DefaultHostTimeout).ShouldBeTrue("The test host did not start before the timeout.");
    }

    public static void WaitForCollector(ManualResetEventSlim resetEvent)
    {
        resetEvent.Wait(DefaultCollectorTimeout).ShouldBeTrue("The health check collector did not complete before the timeout.");
    }

    public static async Task<HealthCheckExecution> WaitForExecutionAsync(HttpClient client, string name)
    {
        var started = Stopwatch.GetTimestamp();
        var timeout = TimeSpan.FromMilliseconds(DefaultCollectorTimeout);
        List<HealthCheckExecution> report = [];

        while (Stopwatch.GetElapsedTime(started) < timeout)
        {
            report = await client.GetAsJson<List<HealthCheckExecution>>("/healthchecks-api").ConfigureAwait(false);
            var execution = report.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));

            if (execution is not null)
            {
                return execution;
            }

            await Task.Delay(250).ConfigureAwait(false);
        }

        throw new TimeoutException($"The health check execution '{name}' was not returned by the UI API before the timeout. Last response contained {report.Count} execution(s).");
    }

    public static Task WaitForMySqlAsync() => WaitForDatabaseAsync(async () =>
    {
        await using var conn = new MySqlConnector.MySqlConnection(MySqlServerConnectionString());
        await conn.OpenAsync();
    });

    public static Task WaitForSqlServerAsync() => WaitForDatabaseAsync(async () =>
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(SqlServerConnectionString());
        await conn.OpenAsync();
    });

    public static Task WaitForPostgresAsync() => WaitForDatabaseAsync(async () =>
    {
        await using var conn = new Npgsql.NpgsqlConnection(PostgresServerConnectionString());
        await conn.OpenAsync();
    });

    private static async Task WaitForDatabaseAsync(Func<Task> tryConnect, int maxAttempts = 30, int delayMs = 1000)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                await tryConnect();
                return;
            }
            catch
            {
                if (i == maxAttempts - 1)
                    throw new TimeoutException("Database did not become available within 30 seconds.");
                await Task.Delay(delayMs);
            }
        }
    }
}
