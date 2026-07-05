using HealthChecks.Minio;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="MinioHealthCheck"/>.
/// </summary>
public static class MinioHealthCheckBuilderExtensions
{
    private const string NAME = "minio";

    /// <summary>
    /// Add a health check for MinIO services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain an <see cref="IMinioClient"/> instance.
    /// When not provided, <see cref="IMinioClient"/> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="MinioHealthCheckOptions"/> used by the health check.
    /// When not provided, the MinIO readiness endpoint is checked.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'minio' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMinio(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IMinioClient>? clientFactory = default,
        Func<IServiceProvider, MinioHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MinioHealthCheck(
                client: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<IMinioClient>(),
                options: optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
