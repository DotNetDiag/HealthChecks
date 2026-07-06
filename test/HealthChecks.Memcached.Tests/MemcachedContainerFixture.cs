using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace HealthChecks.Memcached.Tests;

public sealed class MemcachedContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "library/memcached";

    public const string Tag = "1.6-alpine";

    private const int MemcachedPort = 11211;

    public IContainer? Container { get; private set; }

    public string Host => Container?.Hostname ??
        throw new InvalidOperationException("The test container was not initialized.");

    public int Port => Container?.GetMappedPublicPort(MemcachedPort) ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    public static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithPortBinding(MemcachedPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(MemcachedPort, _ => { }))
            .Build();

        await container.StartAsync();

        return container;
    }
}
