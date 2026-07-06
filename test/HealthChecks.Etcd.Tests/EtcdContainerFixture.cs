using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Etcd.Tests;

public sealed class EtcdContainerFixture : IAsyncLifetime
{
    public const string Registry = "quay.io";

    public const string Image = "coreos/etcd";

    public const string Tag = "v3.5.21";

    private const int ClientPort = 2379;

    private const int PeerPort = 2380;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return $"http://{Container.Hostname}:{Container.GetMappedPublicPort(ClientPort)}";
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
            .WithPortBinding(PeerPort, true)
            .WithCommand(
                "/usr/local/bin/etcd",
                "--name",
                "healthchecks-etcd",
                "--listen-client-urls",
                "http://0.0.0.0:2379",
                "--advertise-client-urls",
                "http://127.0.0.1:2379",
                "--listen-peer-urls",
                "http://0.0.0.0:2380",
                "--initial-advertise-peer-urls",
                "http://127.0.0.1:2380",
                "--initial-cluster",
                "healthchecks-etcd=http://127.0.0.1:2380")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(ClientPort, _ => { }))
            .Build();

        await container.StartAsync();

        return container;
    }
}
