using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using FirebirdSql.Data.FirebirdClient;
using Testcontainers.FirebirdSql;

namespace HealthChecks.Firebird.Tests;

public sealed class FirebirdContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "firebirdsql/firebird";

    public const string Tag = "5.0.4";

    private const string Database = "/var/lib/firebird/data/healthchecks.fdb";
    private const string HealthQuery = "SELECT 1 FROM RDB$DATABASE";

    private const string ReadinessCommand =
        "printf '%s\\n' '" + HealthQuery + ";' | /opt/firebird/bin/isql -user test -password test localhost:" + Database;

    private static readonly TimeSpan ClientConnectionTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ClientConnectionPollInterval = TimeSpan.FromMilliseconds(250);

    public FirebirdSqlContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<FirebirdSqlContainer> CreateContainerAsync()
    {
        var container = new FirebirdSqlBuilder($"{Registry}/{Image}:{Tag}")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithDatabase(Database)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted(["sh", "-c", ReadinessCommand]))
            .Build();

        await container.StartAsync();

        try
        {
            await WaitUntilClientConnectionIsReadyAsync(container);
        }
        catch
        {
            await container.DisposeAsync();
            throw;
        }

        return container;
    }

    private static async Task WaitUntilClientConnectionIsReadyAsync(FirebirdSqlContainer container)
    {
        string connectionString = $"{container.GetConnectionString()};Connection Timeout=1;Pooling=false";
        DateTimeOffset stopAt = DateTimeOffset.UtcNow.Add(ClientConnectionTimeout);
        Exception? lastException = null;

        do
        {
            try
            {
                using var connection = new FbConnection(connectionString);
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = HealthQuery;
                _ = await command.ExecuteScalarAsync();

                return;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (DateTimeOffset.UtcNow >= stopAt)
                    break;

                await Task.Delay(ClientConnectionPollInterval);
            }
        }
        while (true);

        throw new InvalidOperationException("Firebird container did not become available to the client before the timeout elapsed.", lastException);
    }
}
