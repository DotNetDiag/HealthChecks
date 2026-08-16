using Minio;
using Testcontainers.Minio;

namespace HealthChecks.Minio.Tests;

public sealed class MinioContainerFixture : IAsyncLifetime
{
    private const string Image = "minio/minio:RELEASE.2025-09-07T16-13-09Z";

    private readonly MinioContainer _container = new MinioBuilder(Image)
        .Build();

    public IMinioClient CreateClient()
    {
        return new MinioClient()
            .WithEndpoint(new Uri(_container.GetConnectionString()))
            .WithCredentials(_container.GetAccessKey(), _container.GetSecretKey())
            .Build();
    }

    public Task InitializeAsync()
        => _container.StartAsync();

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}
