namespace HealthChecks.Minio;

/// <summary>
/// Specifies the MinIO operation used by <see cref="MinioHealthCheck"/>.
/// </summary>
public enum MinioHealthCheckType
{
    /// <summary>
    /// Checks the MinIO readiness endpoint.
    /// </summary>
    Ready,

    /// <summary>
    /// Checks the MinIO liveness endpoint.
    /// </summary>
    Live,

    /// <summary>
    /// Checks the MinIO cluster health endpoint.
    /// </summary>
    Cluster,

    /// <summary>
    /// Checks the MinIO cluster read quorum endpoint.
    /// </summary>
    ClusterRead,

    /// <summary>
    /// Lists buckets through the MinIO S3-compatible API.
    /// </summary>
    ListBuckets,

    /// <summary>
    /// Checks whether the configured bucket exists through the MinIO S3-compatible API.
    /// </summary>
    BucketExists
}
