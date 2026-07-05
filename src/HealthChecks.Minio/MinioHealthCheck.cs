using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;
using Minio.DataModel.Args;

namespace HealthChecks.Minio;

/// <summary>
/// A health check for MinIO services.
/// </summary>
public sealed class MinioHealthCheck : IHealthCheck
{
    private readonly IMinioClient _client;
    private readonly MinioHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the MinIO health check.
    /// </summary>
    /// <param name="client">The MinIO client used to perform health check operations.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public MinioHealthCheck(IMinioClient client, MinioHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new MinioHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return _options.HealthCheckType switch
            {
                MinioHealthCheckType.BucketExists => await CheckBucketExistsAsync(context, cancellationToken).ConfigureAwait(false),
                MinioHealthCheckType.ListBuckets => await CheckListBucketsAsync(cancellationToken).ConfigureAwait(false),
                _ => await CheckHealthEndpointAsync(context, cancellationToken).ConfigureAwait(false)
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<HealthCheckResult> CheckHealthEndpointAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        Uri endpoint = GetHealthEndpointUri();

        using HttpResponseMessage response = await _client.Config.HttpClient
            .GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Healthy();
        }

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            description: $"MinIO health endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
    }

    private async Task<HealthCheckResult> CheckListBucketsAsync(CancellationToken cancellationToken)
    {
        await _client.ListBucketsAsync(cancellationToken).ConfigureAwait(false);

        return HealthCheckResult.Healthy();
    }

    private async Task<HealthCheckResult> CheckBucketExistsAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"{nameof(MinioHealthCheckOptions.BucketName)} must be configured.");
        }

        var args = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        bool bucketExists = await _client.BucketExistsAsync(args, cancellationToken).ConfigureAwait(false);

        if (bucketExists)
        {
            return HealthCheckResult.Healthy();
        }

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            description: $"Bucket '{_options.BucketName}' does not exist.");
    }

    private Uri GetHealthEndpointUri()
    {
        if (_options.HealthEndpointUri is not null)
        {
            return _options.HealthEndpointUri;
        }

        string endpoint = _client.Config.Endpoint;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("The MinIO client endpoint URI must be configured.");
        }

        var baseUri = new Uri(endpoint, UriKind.Absolute);

        return new Uri(baseUri, GetHealthEndpointPath(_options.HealthCheckType));
    }

    private static string GetHealthEndpointPath(MinioHealthCheckType healthCheckType)
    {
        return healthCheckType switch
        {
            MinioHealthCheckType.Live => "/minio/health/live",
            MinioHealthCheckType.Ready => "/minio/health/ready",
            MinioHealthCheckType.Cluster => "/minio/health/cluster",
            MinioHealthCheckType.ClusterRead => "/minio/health/cluster/read",
            _ => throw new InvalidOperationException($"The {healthCheckType} check type does not use a MinIO health endpoint.")
        };
    }
}
