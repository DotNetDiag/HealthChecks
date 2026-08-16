namespace HealthChecks.Minio;

/// <summary>
/// Represents settings used by <see cref="MinioHealthCheck"/>.
/// </summary>
public sealed class MinioHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the MinIO operation used by the health check.
    /// </summary>
    public MinioHealthCheckType HealthCheckType { get; set; } = MinioHealthCheckType.Ready;

    /// <summary>
    /// Gets or sets the bucket name used when <see cref="HealthCheckType"/> is <see cref="MinioHealthCheckType.BucketExists"/>.
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the exact MinIO health endpoint URI used for HTTP endpoint checks.
    /// </summary>
    /// <remarks>
    /// When this value is not configured, the URI is built from the configured <c>IMinioClient</c> endpoint and <see cref="HealthCheckType"/>.
    /// </remarks>
    public Uri? HealthEndpointUri { get; set; }
}
