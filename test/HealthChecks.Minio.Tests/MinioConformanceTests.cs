using Minio;

namespace HealthChecks.Minio.Tests;

public class MinioConformanceTests : ConformanceTests<IMinioClient, MinioHealthCheck, MinioHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, IMinioClient>? clientFactory = default,
        Func<IServiceProvider, MinioHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddMinio(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override IMinioClient CreateClientForNonExistingEndpoint()
        => new MinioClient()
            .WithEndpoint("127.0.0.1", 1)
            .WithCredentials("access-key", "secret-key")
            .WithSSL(false)
            .Build();

    protected override MinioHealthCheck CreateHealthCheck(IMinioClient client, MinioHealthCheckOptions? options)
        => new(client, options);

    protected override MinioHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
