using HealthChecks.CockroachDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="CockroachDbHealthCheck"/>.
/// </summary>
public static class CockroachDbHealthCheckBuilderExtensions
{
    private const string NAME = "cockroachdb";
    internal const string HEALTH_QUERY = "SELECT 1;";

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The CockroachDB connection string to be used.</param>
    /// <param name="setup">An optional action to configure CockroachDB health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        string connectionString,
        Action<CockroachDbHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);

        return builder.AddCockroachDb(
            _ =>
            {
                var options = new CockroachDbHealthCheckOptions(connectionString);
                setup?.Invoke(options);
                return options;
            },
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the CockroachDB connection string to use.</param>
    /// <param name="setup">An optional action to configure CockroachDB health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Action<CockroachDbHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.AddCockroachDb(
            sp =>
            {
                var options = new CockroachDbHealthCheckOptions(
                    Guard.ThrowIfNull(connectionStringFactory(sp), throwOnEmptyString: true, paramName: nameof(connectionStringFactory)));

                setup?.Invoke(options);
                return options;
            },
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="dataSource">The CockroachDB <see cref="NpgsqlDataSource" /> to be used.</param>
    /// <param name="setup">An optional action to configure CockroachDB health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        NpgsqlDataSource dataSource,
        Action<CockroachDbHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(dataSource);

        return builder.AddCockroachDb(
            _ =>
            {
                var options = new CockroachDbHealthCheckOptions(dataSource);
                setup?.Invoke(options);
                return options;
            },
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="dataSourceFactory">
    /// An optional factory to obtain <see cref="NpgsqlDataSource" /> instance.
    /// When not provided, <see cref="NpgsqlDataSource" /> is resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="setup">An optional action to configure CockroachDB health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, NpgsqlDataSource>? dataSourceFactory = default,
        Action<CockroachDbHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        return builder.AddCockroachDb(
            sp =>
            {
                var options = new CockroachDbHealthCheckOptions(dataSourceFactory?.Invoke(sp) ?? sp.GetRequiredService<NpgsqlDataSource>());
                setup?.Invoke(options);
                return options;
            },
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for a CockroachDB node health endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="nodeHealthEndpoint">The CockroachDB node health endpoint to be used.</param>
    /// <param name="setup">An optional action to configure CockroachDB health check options.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        Uri nodeHealthEndpoint,
        Action<CockroachDbHealthCheckOptions>? setup = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(nodeHealthEndpoint);

        return builder.AddCockroachDb(
            _ =>
            {
                var options = new CockroachDbHealthCheckOptions(nodeHealthEndpoint);
                setup?.Invoke(options);
                return options;
            },
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        CockroachDbHealthCheckOptions options,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(options);

        return builder.AddCockroachDb(
            _ => options,
            name,
            failureStatus,
            tags,
            timeout,
            configureClient,
            configurePrimaryHttpMessageHandler);
    }

    /// <summary>
    /// Add a health check for CockroachDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="optionsFactory">A factory to build the CockroachDB health check options to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'cockroachdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="configureClient">An optional setup action to configure the CockroachDB node health check HTTP client.</param>
    /// <param name="configurePrimaryHttpMessageHandler">An optional setup action to configure the CockroachDB node health check HTTP client message handler.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCockroachDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, CockroachDbHealthCheckOptions> optionsFactory,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        Action<IServiceProvider, HttpClient>? configureClient = default,
        Func<IServiceProvider, HttpMessageHandler>? configurePrimaryHttpMessageHandler = default)
    {
        Guard.ThrowIfNull(optionsFactory);

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

        CockroachDbHealthCheckOptions? options = null;

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp =>
            {
                options ??= Guard.ThrowIfNull(optionsFactory(sp), paramName: nameof(optionsFactory));
                HttpClient? httpClient = options.NodeHealthEndpoint is null
                    ? null
                    : sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName);

                return new CockroachDbHealthCheck(options, httpClient);
            },
            failureStatus,
            tags,
            timeout));
    }
}
