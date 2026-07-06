using DotNet.Testcontainers.Builders;
using Testcontainers.FirebirdSql;

namespace HealthChecks.Firebird.Tests;

public sealed class FirebirdContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "firebirdsql/firebird";

    public const string Tag = "5.0.4";

    private const int FirebirdPort = 3050;

    private const string Database = "/var/lib/firebird/data/healthchecks.fdb";

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
            .WithDatabase(Database)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(FirebirdPort, _ => { }))
            .Build();

        await container.StartAsync();

        return container;
    }
}
