using Apache.NMS;
using HealthChecks.Artemis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ArtemisHealthCheck"/>.
/// </summary>
public static class ArtemisHealthCheckBuilderExtensions
{
    private const string NAME = "artemis";

    /// <summary>
    /// Add a health check for Apache ActiveMQ Artemis brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="brokerUri">The Artemis AMQP broker URI, for example <c>amqp://localhost:61616</c>.</param>
    /// <param name="setup">An optional action to configure Artemis health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'artemis' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddArtemis(
        this IHealthChecksBuilder builder,
        string brokerUri,
        Action<ArtemisHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(brokerUri, throwOnEmptyString: true);

        return builder.AddArtemis(new Uri(brokerUri, UriKind.Absolute), setup, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Artemis brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="brokerUri">The Artemis AMQP broker URI, for example <c>amqp://localhost:61616</c>.</param>
    /// <param name="setup">An optional action to configure Artemis health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'artemis' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddArtemis(
        this IHealthChecksBuilder builder,
        Uri brokerUri,
        Action<ArtemisHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(brokerUri);

        return builder.AddArtemis(
            _ => new Apache.NMS.AMQP.ConnectionFactory(brokerUri),
            _ => CreateOptions(setup),
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Artemis brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">The Artemis connection factory used to connect to the broker.</param>
    /// <param name="setup">An optional action to configure Artemis health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'artemis' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddArtemis(
        this IHealthChecksBuilder builder,
        IConnectionFactory connectionFactory,
        Action<ArtemisHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionFactory);

        return builder.AddArtemis(_ => connectionFactory, _ => CreateOptions(setup), name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Artemis brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">
    /// An optional factory to obtain <see cref="IConnectionFactory" /> instance.
    /// When not provided, <see cref="IConnectionFactory" /> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">An optional factory to obtain health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'artemis' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddArtemis(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IConnectionFactory>? connectionFactory = default,
        Func<IServiceProvider, ArtemisHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ArtemisHealthCheck(
                connectionFactory?.Invoke(sp) ?? sp.GetRequiredService<IConnectionFactory>(),
                optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }

    private static ArtemisHealthCheckOptions CreateOptions(Action<ArtemisHealthCheckOptions>? setup)
    {
        var options = new ArtemisHealthCheckOptions();
        setup?.Invoke(options);
        return options;
    }
}
