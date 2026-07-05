using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Gcp.CloudStorage;

/// <summary>
/// Google Cloud Storage health check.
/// </summary>
public sealed class CloudStorageHealthCheck : IHealthCheck
{
    private readonly StorageClient _storageClient;
    private readonly CloudStorageHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the Google Cloud Storage health check.
    /// </summary>
    /// <param name="storageClient">
    /// The <see cref="StorageClient"/> used to perform Google Cloud Storage operations.
    /// Google Cloud clients should generally be reused, so this should be the same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public CloudStorageHealthCheck(StorageClient storageClient, CloudStorageHealthCheckOptions? options = default)
    {
        _storageClient = Guard.ThrowIfNull(storageClient);
        _options = options ?? new CloudStorageHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(_options.BucketName))
            {
                await _storageClient
                    .GetBucketAsync(_options.BucketName, _options.GetBucketOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(_options.ProjectId))
            {
                await _storageClient
                    .ListBucketsAsync(_options.ProjectId, _options.ListBucketsOptions ?? new ListBucketsOptions { PageSize = 1 })
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"{nameof(CloudStorageHealthCheckOptions.ProjectId)} or {nameof(CloudStorageHealthCheckOptions.BucketName)} must be configured.");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
