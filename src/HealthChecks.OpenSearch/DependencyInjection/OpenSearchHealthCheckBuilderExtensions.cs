using HealthChecks.OpenSearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="OpenSearchHealthCheck"/>.
/// </summary>
public static class OpenSearchHealthCheckBuilderExtensions
{
    private const string NAME = "opensearch";

    /// <summary>
    /// Add a health check for OpenSearch services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="openSearchUri">The OpenSearch server URI to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'opensearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="useClusterHealthApi">When <c>true</c>, uses the cluster health API instead of the ping API.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddOpenSearch(
        this IHealthChecksBuilder builder,
        string openSearchUri,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        bool useClusterHealthApi = default)
    {
        return builder.AddOpenSearch(
            options =>
            {
                options.UseServer(openSearchUri);
                options.UseClusterHealthApi = useClusterHealthApi;
            },
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for OpenSearch services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The OpenSearch option setup.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'opensearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddOpenSearch(
        this IHealthChecksBuilder builder,
        Action<OpenSearchOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new OpenSearchOptions();
        setup?.Invoke(options);

        options.RequestTimeout ??= timeout;

        if (options.Uri is null)
        {
            throw new InvalidOperationException($"There is no server to connect. Consider using {nameof(OpenSearchOptions.UseServer)}.");
        }

        var client = new Lazy<IOpenSearchClient>(() => CreateClient(options));

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            _ => new OpenSearchHealthCheck(client.Value, options.UseClusterHealthApi),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for OpenSearch services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="IOpenSearchClient" /> instance.
    /// When not provided, <see cref="IOpenSearchClient" /> or <see cref="OpenSearchClient" /> is resolved from <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'opensearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <param name="useClusterHealthApi">When <c>true</c>, uses the cluster health API instead of the ping API.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddOpenSearch(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IOpenSearchClient>? clientFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default,
        bool useClusterHealthApi = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new OpenSearchHealthCheck(clientFactory?.Invoke(sp) ?? ResolveClient(sp), useClusterHealthApi),
            failureStatus,
            tags,
            timeout));
    }

    private static IOpenSearchClient ResolveClient(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<IOpenSearchClient>() ?? serviceProvider.GetRequiredService<OpenSearchClient>();
    }

    private static IOpenSearchClient CreateClient(OpenSearchOptions options)
    {
        var settings = new ConnectionSettings(options.Uri);

        if (options.RequestTimeout.HasValue)
        {
            settings = settings.RequestTimeout(options.RequestTimeout.Value);
        }

        if (options.AuthenticateWithBasicCredentials)
        {
            settings = settings.BasicAuthentication(options.UserName!, options.Password!);
        }
        else if (options.AuthenticateWithApiKey)
        {
            settings = settings.ApiKeyAuthentication(options.ApiKeyId!, options.ApiKey!);
        }
        else if (options.AuthenticateWithCertificate)
        {
            settings = settings.ClientCertificate(options.Certificate!);
        }

        if (options.CertificateValidationCallback is not null)
        {
            settings = settings.ServerCertificateValidationCallback(options.CertificateValidationCallback);
        }

        return new OpenSearchClient(settings);
    }
}
