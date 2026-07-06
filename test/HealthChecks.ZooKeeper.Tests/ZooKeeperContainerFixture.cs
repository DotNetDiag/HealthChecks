using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.ZooKeeper.Tests;

public sealed class ZooKeeperContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "zookeeper";

    public const string Tag = "3.9.4";

    private const int ClientPort = 2181;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return $"{Container.Hostname}:{Container.GetMappedPublicPort(ClientPort)}";
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithPortBinding(ClientPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(ClientPort, _ => { }))
            .Build();

        await container.StartAsync();

        return container;
    }
}
