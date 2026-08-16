using Testcontainers.Neo4j;

namespace HealthChecks.Neo4j.Tests;

public sealed class Neo4jContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/neo4j";

    private const string Tag = "5.26-community";

    public Neo4jContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString()
        ?? throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<Neo4jContainer> CreateContainerAsync()
    {
        var container = new Neo4jBuilder($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
