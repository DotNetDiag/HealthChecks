using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ContainerRegistry;

/// <summary>
/// A health check for OCI and Docker Registry HTTP API v2 endpoints.
/// </summary>
public sealed class ContainerRegistryHealthCheck : IHealthCheck
{
    private const string DOCKER_DISTRIBUTION_API_VERSION_HEADER = "Docker-Distribution-API-Version";

    private readonly HttpClient _client;
    private readonly ContainerRegistryHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the container registry health check.
    /// </summary>
    /// <param name="client">The HTTP client used to call the registry API.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public ContainerRegistryHealthCheck(HttpClient client, ContainerRegistryHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new ContainerRegistryHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetRegistryEndpointUri());
            _options.ConfigureRequest?.Invoke(request);

            using HttpResponseMessage response = await _client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            Dictionary<string, object> data = CreateData(response);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(data: data);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized && _options.AllowUnauthorizedResponse)
            {
                if (!_options.RequireAuthenticationChallenge || response.Headers.WwwAuthenticate.Count != 0)
                {
                    return HealthCheckResult.Healthy(data: data);
                }

                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: "Container registry endpoint returned HTTP 401 (Unauthorized) without a WWW-Authenticate challenge.",
                    data: data);
            }

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Container registry endpoint returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).",
                data: data);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private Uri GetRegistryEndpointUri()
    {
        if (string.IsNullOrWhiteSpace(_options.RegistryEndpointPath))
        {
            throw new InvalidOperationException($"{nameof(ContainerRegistryHealthCheckOptions.RegistryEndpointPath)} must be configured.");
        }

        if (Uri.TryCreate(_options.RegistryEndpointPath, UriKind.Absolute, out Uri? absoluteUri) && !absoluteUri.IsFile)
        {
            return absoluteUri;
        }

        Uri? baseUri = (_options.BaseUri ?? _client.BaseAddress) ?? throw new InvalidOperationException($"{nameof(ContainerRegistryHealthCheckOptions.BaseUri)} or {nameof(HttpClient.BaseAddress)} must be configured.");

        return new Uri(baseUri, _options.RegistryEndpointPath);
    }

    private static Dictionary<string, object> CreateData(HttpResponseMessage response)
    {
        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "statusCode", (int)response.StatusCode },
            { "authenticationChallenge", response.Headers.WwwAuthenticate.Count != 0 }
        };

        if (response.Headers.TryGetValues(DOCKER_DISTRIBUTION_API_VERSION_HEADER, out IEnumerable<string>? versions))
        {
            data["distributionApiVersion"] = string.Join(",", versions);
        }

        if (response.Headers.WwwAuthenticate.Count != 0)
        {
            data["wwwAuthenticate"] = string.Join(",", response.Headers.WwwAuthenticate);
        }

        return data;
    }
}
