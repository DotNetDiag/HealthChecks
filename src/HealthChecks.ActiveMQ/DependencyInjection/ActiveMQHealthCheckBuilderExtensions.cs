using Apache.NMS;
using HealthChecks.ActiveMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ActiveMQHealthCheck"/>.
/// </summary>
public static class ActiveMQHealthCheckBuilderExtensions
{
    private const string NAME = "activemq";

    /// <summary>
    /// Add a health check for Apache ActiveMQ Classic brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="brokerUri">The ActiveMQ broker URI, for example <c>activemq:tcp://localhost:61616</c>.</param>
    /// <param name="setup">An optional action to configure ActiveMQ health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'activemq' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        string brokerUri,
        Action<ActiveMQHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(brokerUri, throwOnEmptyString: true);

        return builder.AddActiveMQ(new Uri(brokerUri, UriKind.Absolute), setup, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Classic brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="brokerUri">The ActiveMQ broker URI, for example <c>activemq:tcp://localhost:61616</c>.</param>
    /// <param name="setup">An optional action to configure ActiveMQ health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'activemq' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        Uri brokerUri,
        Action<ActiveMQHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(brokerUri);

        return builder.AddActiveMQ(
            _ => new Apache.NMS.ActiveMQ.ConnectionFactory(brokerUri),
            _ => CreateOptions(setup),
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Classic brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">The ActiveMQ connection factory used to connect to the broker.</param>
    /// <param name="setup">An optional action to configure ActiveMQ health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'activemq' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        IConnectionFactory connectionFactory,
        Action<ActiveMQHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionFactory);

        return builder.AddActiveMQ(_ => connectionFactory, _ => CreateOptions(setup), name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Apache ActiveMQ Classic brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionFactory">
    /// An optional factory to obtain <see cref="IConnectionFactory" /> instance.
    /// When not provided, <see cref="IConnectionFactory" /> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">An optional factory to obtain health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'activemq' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IConnectionFactory>? connectionFactory = default,
        Func<IServiceProvider, ActiveMQHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ActiveMQHealthCheck(
                connectionFactory?.Invoke(sp) ?? sp.GetRequiredService<IConnectionFactory>(),
                optionsFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }

    private static ActiveMQHealthCheckOptions CreateOptions(Action<ActiveMQHealthCheckOptions>? setup)
    {
        var options = new ActiveMQHealthCheckOptions();
        setup?.Invoke(options);
        return options;
    }
}
