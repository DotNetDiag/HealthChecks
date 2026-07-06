using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.CockroachDb.Tests;

public sealed class CockroachDbContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "cockroachdb/cockroach";

    private const string Tag = "v26.2.3";

    private const int SqlPort = 26257;

    private const int HttpPort = 8080;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return $"Host={Container.Hostname};Port={Container.GetMappedPublicPort(SqlPort)};Username=root;Database=defaultdb;SSL Mode=Disable";
    }

    public Uri GetNodeHealthEndpoint()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(HttpPort), "/health")
        {
            Query = "ready=1"
        }.Uri;
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithPortBinding(SqlPort, true)
            .WithPortBinding(HttpPort, true)
            .WithCommand(
                "start-single-node",
                "--insecure")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
                .ForPort(HttpPort)
                .ForPath("/health?ready=1")))
            .Build();

        await container.StartAsync();

        return container;
    }
}
