using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace HealthChecks.SonnetDB.Tests;

public sealed class SonnetDBContainerFixture : IAsyncLifetime
{
    private const string Image = "iotsharp/sonnetdb:latest";

    private const int HttpPort = 5080;

    public IContainer? Container { get; private set; }

    public Uri GetBaseUri()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(HttpPort)).Uri;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder(Image)
            .WithImagePullPolicy(PullPolicy.Always)
            .WithPortBinding(HttpPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
                .ForPort(HttpPort)
                .ForPath("/healthz")))
            .Build();

        await container.StartAsync();

        return container;
    }
}
