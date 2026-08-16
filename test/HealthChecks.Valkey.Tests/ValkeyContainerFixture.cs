using Testcontainers.Redis;

namespace HealthChecks.Valkey.Tests;

public sealed class ValkeyContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "valkey/valkey";

    public const string Tag = "8.0";

    public RedisContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }

    public static async Task<RedisContainer> CreateContainerAsync()
    {
        var container = new RedisBuilder($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}

