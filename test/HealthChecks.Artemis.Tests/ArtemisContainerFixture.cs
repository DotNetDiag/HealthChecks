using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace HealthChecks.Artemis.Tests;

public sealed class ArtemisContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "apache/artemis";

    private const string Tag = "2.55.0-alpine";

    private const int BrokerPort = 61616;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return $"amqp://{Container.Hostname}:{Container.GetMappedPublicPort(BrokerPort)}";
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithPortBinding(BrokerPort, true)
            .WithEnvironment("ARTEMIS_USER", "artemis")
            .WithEnvironment("ARTEMIS_PASSWORD", "artemis")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilExternalTcpPortIsAvailable(BrokerPort, _ => { })
                .UntilMessageIsLogged("AMQ221007: Server is now active"))
            .Build();

        await container.StartAsync();

        return container;
    }
}
