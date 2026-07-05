using Google.Cloud.Storage.V1;
using HealthChecks.Gcp.CloudStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="CloudStorageHealthCheck"/>.
/// </summary>
public static class CloudStorageHealthChecksBuilderExtensions
{
    private const string NAME = "gcp_cloud_storage";

    /// <summary>
    /// Add a health check for Google Cloud Storage by registering <see cref="CloudStorageHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="StorageClient"/> instance.
    /// When not provided, <see cref="StorageClient"/> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="CloudStorageHealthCheckOptions"/> used by the health check.
    /// When not provided, defaults are used.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'gcp_cloud_storage' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudStorage(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, StorageClient>? clientFactory = default,
        Func<IServiceProvider, CloudStorageHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new CloudStorageHealthCheck(
                storageClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<StorageClient>(),
                options: optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
