using HealthChecks.NTPServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="NTPServerHealthCheck"/>.
/// </summary>
public static class NTPServerHealthCheckBuilderExtensions
{
    private const string NAME = "ntpserver";

    /// <summary>
    /// Add a health check for NTP servers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">An optional action to configure the <see cref="NTPServerHealthCheckOptions"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ntpserver' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNTPServer(
        this IHealthChecksBuilder builder,
        Action<NTPServerHealthCheckOptions>? setup = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new NTPServerHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new NTPServerHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
