using DotPulsar.Abstractions;
using HealthChecks.Apache.Pulsar;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure Apache Pulsar health checks.
/// </summary>
public static class PulsarHealthCheckBuilderExtensions
{
    private const string NAME = "pulsar";
    private const string ADMIN_NAME = "pulsar_admin";

    /// <summary>
    /// Add a health check for Apache Pulsar brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="client">The Pulsar client used to verify broker connectivity.</param>
    /// <param name="setup">An optional action to configure Apache Pulsar health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'pulsar' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        IPulsarClient client,
        Action<PulsarHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(client);

        return builder.AddPulsar(
            clientFactory: _ => client,
            optionsFactory: _ =>
            {
                var options = new PulsarHealthCheckOptions();
                setup?.Invoke(options);
                return options;
            },
            name: name,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout);
    }

    /// <summary>
    /// Add a health check for Apache Pulsar brokers.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain an <see cref="IPulsarClient"/> instance.
    /// When not provided, <see cref="IPulsarClient"/> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="PulsarHealthCheckOptions"/> used by the health check.
    /// When not provided, the default health check topic is used.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'pulsar' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IPulsarClient>? clientFactory = default,
        Func<IServiceProvider, PulsarHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        string registrationName = name ?? NAME;

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => CreateHealthCheck(sp, clientFactory, optionsFactory),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Apache Pulsar admin endpoints.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceUri">The Apache Pulsar admin service URI.</param>
    /// <param name="setup">An optional action to configure Apache Pulsar admin health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'pulsar_admin' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the Apache Pulsar admin health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the Apache Pulsar admin health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsarAdmin(
        this IHealthChecksBuilder builder,
        Uri serviceUri,
        Action<PulsarAdminHealthCheckOptions>? setup = default,
        string? name = ADMIN_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(serviceUri);
        string registrationName = name ?? ADMIN_NAME;

        return builder.AddPulsarAdmin(
            clientFactory: sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName),
            optionsFactory: _ =>
            {
                var options = new PulsarAdminHealthCheckOptions();
                setup?.Invoke(options);
                return options;
            },
            name: registrationName,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout,
            configureClient: (sp, client) =>
            {
                client.BaseAddress = serviceUri;
                configureClient?.Invoke(sp, client);
            },
            configurePrimaryHttpMessageHandler: configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for Apache Pulsar admin endpoints.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain an <see cref="HttpClient"/> instance.
    /// When not provided, a registered <see cref="HttpClient"/> or the named <see cref="IHttpClientFactory"/> client is used.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="PulsarAdminHealthCheckOptions"/> used by the health check.
    /// When not provided, the default Apache Pulsar admin health endpoint is used.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'pulsar_admin' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the Apache Pulsar admin health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the Apache Pulsar admin health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsarAdmin(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, PulsarAdminHealthCheckOptions>? optionsFactory = default,
        string? name = ADMIN_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        string registrationName = name ?? ADMIN_NAME;

        IHttpClientBuilder httpClientBuilder = builder.Services.AddHttpClient(registrationName);

        if (configureClient is not null)
        {
            httpClientBuilder.ConfigureHttpClient(configureClient);
        }

        if (configurePrimaryHttpMessageHandler is not null)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler);
        }

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => CreateAdminHealthCheck(sp, registrationName, clientFactory, optionsFactory),
            failureStatus,
            tags,
            timeout));
    }

    private static PulsarHealthCheck CreateHealthCheck(
        IServiceProvider sp,
        Func<IServiceProvider, IPulsarClient>? clientFactory,
        Func<IServiceProvider, PulsarHealthCheckOptions>? optionsFactory)
    {
        IPulsarClient client = clientFactory?.Invoke(sp) ?? sp.GetRequiredService<IPulsarClient>();
        return new PulsarHealthCheck(client, optionsFactory?.Invoke(sp));
    }

    private static PulsarAdminHealthCheck CreateAdminHealthCheck(
        IServiceProvider sp,
        string name,
        Func<IServiceProvider, HttpClient>? clientFactory,
        Func<IServiceProvider, PulsarAdminHealthCheckOptions>? optionsFactory)
    {
        HttpClient client = clientFactory?.Invoke(sp)
            ?? sp.GetService<HttpClient>()
            ?? sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);

        return new PulsarAdminHealthCheck(client, optionsFactory?.Invoke(sp));
    }
}
