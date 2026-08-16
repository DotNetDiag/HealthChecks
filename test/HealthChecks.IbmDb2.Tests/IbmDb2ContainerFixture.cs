using Testcontainers.Db2;

namespace HealthChecks.IbmDb2.Tests;

public sealed class IbmDb2ContainerFixture : IAsyncLifetime
{
    public const string Registry = "icr.io";

    public const string Image = "db2_community/db2";

    public const string Tag = "12.1.0.0";

    public Db2Container? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync()
    {
        if (!OperatingSystem.IsLinux())
            return;

        Container = await CreateContainerAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public static async Task<Db2Container> CreateContainerAsync()
    {
        var container = new Db2Builder($"{Registry}/{Image}:{Tag}")
            .WithAcceptLicenseAgreement(true)
            .Build();

        await container.StartAsync();

        return container;
    }
}
