using Testcontainers.Pulsar;

namespace HealthChecks.Apache.Pulsar.Tests;

public sealed class PulsarContainerFixture : IAsyncLifetime
{
    public const string Registry = "docker.io";

    public const string Image = "apachepulsar/pulsar";

    public const string Tag = "3.0.9";

    public PulsarContainer? Container { get; private set; }

    public string GetPulsarBrokerUrl() => Container?.GetBrokerAddress() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public string GetHttpServiceUrl() => Container?.GetServiceAddress() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync()
    {
        Container = await CreateContainerAsync();
        await WaitUntilAdminHealthEndpointIsReadyAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<PulsarContainer> CreateContainerAsync()
    {
        var container = new PulsarBuilder($"{Registry}/{Image}:{Tag}")
            .Build();
        await container.StartAsync();

        return container;
    }

    private async Task WaitUntilAdminHealthEndpointIsReadyAsync()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(GetHttpServiceUrl()) };
        var deadline = DateTime.UtcNow.AddSeconds(60);
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync("/admin/v2/brokers/health");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new InvalidOperationException("The Apache Pulsar admin health endpoint did not become ready in time.", lastException);
    }
}
