using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.OpenSearch.Tests;

public sealed class OpenSearchContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "opensearchproject/opensearch";

    private const string Tag = "2.19.0";

    private const int Port = 9200;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(Port)).ToString();
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilHttpRequestIsSucceeded(x => x
                .ForPath("/")
                .ForPort(Port)
                .ForStatusCode(HttpStatusCode.OK));

        var container = new ContainerBuilder($"{Registry}/{Image}:{Tag}")
            .WithPortBinding(Port, true)
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("plugins.security.disabled", "true")
            .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithEnvironment("OPENSEARCH_INITIAL_ADMIN_PASSWORD", "OpenSearchAdmin123!Strong")
            .WithWaitStrategy(waitStrategy)
            .Build();

        await container.StartAsync();

        return container;
    }
}
