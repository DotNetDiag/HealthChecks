using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Testcontainers.FirebirdSql;

namespace HealthChecks.Firebird.Tests;

public sealed class FirebirdContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "firebirdsql/firebird";

    public const string Tag = "5.0.4";

    private const string Database = "/var/lib/firebird/data/healthchecks.fdb";

    private const string ReadinessCommand =
        "printf '%s\\n' 'SELECT 1 FROM RDB$DATABASE;' | /opt/firebird/bin/isql -user test -password test localhost:" + Database;

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

        return container;
    }
}
