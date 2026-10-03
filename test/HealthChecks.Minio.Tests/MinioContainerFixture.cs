using DotNet.Testcontainers.Configurations;
using Minio;
using Testcontainers.Minio;

namespace HealthChecks.Minio.Tests;

public sealed class MinioContainerFixture : IAsyncLifetime
{
    // Official MinIO images are no longer distributed; this legacy vendor image is only used by the test fixture.
    private const string Image = "bitnamilegacy/minio:2025.7.23@sha256:8935e75fa5d11295c17171e4aa49efe390a1193cd7f12e4d21b92af9ffef09d7";

    private readonly MinioContainer _container = new MinioBuilder(Image)
        .WithEntrypoint("/opt/bitnami/minio/bin/minio")
        .WithCommand(new OverwriteEnumerable<string>(["server", "/tmp/minio"]))
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
