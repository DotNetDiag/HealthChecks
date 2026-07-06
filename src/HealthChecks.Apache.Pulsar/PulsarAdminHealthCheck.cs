using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Apache.Pulsar;

/// <summary>
/// A health check for Apache Pulsar admin endpoints.
/// </summary>
public sealed class PulsarAdminHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly PulsarAdminHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="PulsarAdminHealthCheck"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to query the Apache Pulsar admin endpoint.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public PulsarAdminHealthCheck(HttpClient httpClient, PulsarAdminHealthCheckOptions? options = default)
    {
        _httpClient = Guard.ThrowIfNull(httpClient);
        _options = options ?? new PulsarAdminHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateOptions();

            using var request = new HttpRequestMessage(HttpMethod.Get, _options.HealthEndpoint);
            using HttpResponseMessage response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!_options.ResponseValidator(response))
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"Apache Pulsar admin health endpoint returned HTTP {(int)response.StatusCode} ({response.StatusCode}).");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.HealthEndpoint))
        {
            throw new InvalidOperationException($"{nameof(PulsarAdminHealthCheckOptions.HealthEndpoint)} must be configured.");
        }

        if (_options.ResponseValidator is null)
        {
            throw new InvalidOperationException($"{nameof(PulsarAdminHealthCheckOptions.ResponseValidator)} must be configured.");
        }
    }
}
