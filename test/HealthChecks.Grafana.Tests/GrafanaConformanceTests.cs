namespace HealthChecks.Grafana.Tests;

public class GrafanaConformanceTests : ConformanceTests<HttpClient, GrafanaHealthCheck, GrafanaHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, GrafanaHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddGrafana(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override GrafanaHealthCheck CreateHealthCheck(HttpClient client, GrafanaHealthCheckOptions? options)
        => new(client, options);

    protected override GrafanaHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
