using Testcontainers.Cassandra;

namespace HealthChecks.Cassandra.Tests;

public class CassandraContainerFixture : IAsyncLifetime
{
    private const string Image = "cassandra:5";

    public CassandraContainer? Container { get; private set; }

    public string GetContactPoint()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.Hostname;
    }

    public int GetPort()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetMappedPublicPort(CassandraBuilder.CqlPort);
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<CassandraContainer> CreateContainerAsync()
    {
        var container = new CassandraBuilder(Image)
            .Build();

        await container.StartAsync();

        return container;
    }
}
