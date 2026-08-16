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
        await WaitUntilMemcachedRespondsAsync(container);

        return container;
    }

    private static async Task WaitUntilMemcachedRespondsAsync(IContainer container)
    {
        DateTimeOffset timeoutAt = DateTimeOffset.UtcNow.AddSeconds(30);
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            try
            {
                using var client = MemcachedClientFactory.Create(container.Hostname, container.GetMappedPublicPort(MemcachedPort), TimeSpan.FromSeconds(1));

                string key = $"healthchecks_memcached_ready_{Guid.NewGuid():N}";
                string expectedValue = Guid.NewGuid().ToString("N");

                bool stored = await client.SetAsync(key, expectedValue, TimeSpan.FromSeconds(5));
                if (stored)
                {
                    string? actualValue = await client.GetValueAsync<string>(key);
                    await client.RemoveAsync(key);

                    if (string.Equals(expectedValue, actualValue, StringComparison.Ordinal))
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new InvalidOperationException("The Memcached test container did not become ready.", lastException);
    }
}
