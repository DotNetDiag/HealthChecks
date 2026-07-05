using Google.Cloud.Storage.V1;

namespace HealthChecks.Gcp.CloudStorage;

/// <summary>
/// Represents a collection of settings that configure a
/// <see cref="CloudStorageHealthCheck">Google Cloud Storage health check</see>.
/// </summary>
public sealed class CloudStorageHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the Google Cloud project ID used when checking Google Cloud Storage service reachability.
    /// </summary>
    /// <remarks>
    /// This value is required when <see cref="BucketName"/> is not configured.
    /// </remarks>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the name of the Google Cloud Storage bucket whose health should be checked.
    /// </summary>
    /// <remarks>
    /// When this value is configured, the health check fetches this bucket's metadata instead of listing buckets for <see cref="ProjectId"/>.
    /// </remarks>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the options used when fetching configured bucket metadata.
    /// </summary>
    public GetBucketOptions? GetBucketOptions { get; set; }

    /// <summary>
    /// Gets or sets the options used when listing buckets for configured <see cref="ProjectId"/>.
    /// </summary>
    public ListBucketsOptions? ListBucketsOptions { get; set; }
}
