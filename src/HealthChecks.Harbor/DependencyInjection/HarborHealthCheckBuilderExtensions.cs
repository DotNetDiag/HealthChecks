using HealthChecks.Harbor;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="HarborHealthCheck"/>.
/// </summary>
public static class HarborHealthCheckBuilderExtensions
{
    private const string NAME = "harbor";

    /// <summary>
    /// Add a health check for Harbor services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="baseUri">The Harbor base URI.</param>
    /// <param name="setup">An optional action to configure Harbor health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'harbor' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the Harbor health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the Harbor health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddHarbor(
        this IHealthChecksBuilder builder,
        Uri baseUri,
        Action<HarborHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(baseUri);

        return builder.AddHarbor(
            optionsFactory: _ =>
            {
                var options = new HarborHealthCheckOptions
                {
                    BaseUri = baseUri
                };

                setup?.Invoke(options);

                return options;
            },
            name: name,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout,
            configureClient: configureClient,
            configurePrimaryHttpMessageHandler: configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for Harbor services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain an <see cref="HttpClient"/> instance.
    /// When not provided, a registered <see cref="HttpClient"/> or the named <see cref="IHttpClientFactory"/> client is used.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="HarborHealthCheckOptions"/> used by the health check.
    /// When not provided, the Harbor health endpoint is checked using the HTTP client's <see cref="HttpClient.BaseAddress"/>.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'harbor' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the Harbor health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the Harbor health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddHarbor(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, HarborHealthCheckOptions>? optionsFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        string registrationName = name ?? NAME;

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
            sp => CreateHealthCheck(sp, registrationName, clientFactory, optionsFactory),
            failureStatus,
            tags,
            timeout));
    }

    private static HarborHealthCheck CreateHealthCheck(
        IServiceProvider sp,
        string name,
        Func<IServiceProvider, HttpClient>? clientFactory,
        Func<IServiceProvider, HarborHealthCheckOptions>? optionsFactory)
    {
        HttpClient client = clientFactory?.Invoke(sp)
            ?? sp.GetService<HttpClient>()
            ?? sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);

        return new HarborHealthCheck(client, optionsFactory?.Invoke(sp));
    }
}
