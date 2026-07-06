using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace HealthChecks.ActiveMQ.Tests;

public sealed class ActiveMQContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "apache/activemq";

    private const string Tag = "5.19.7";

    private const int BrokerPort = 61616;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return $"activemq:tcp://{Container.Hostname}:{Container.GetMappedPublicPort(BrokerPort)}";
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithPortBinding(BrokerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilExternalTcpPortIsAvailable(BrokerPort, _ => { })
                .UntilMessageIsLogged("Connector openwire started"))
            .Build();

        await container.StartAsync();

        return container;
    }
}
