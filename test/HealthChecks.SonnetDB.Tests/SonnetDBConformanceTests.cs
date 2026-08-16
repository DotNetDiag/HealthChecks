namespace HealthChecks.SonnetDB.Tests;

public class SonnetDBConformanceTests : ConformanceTests<HttpClient, SonnetDBHealthCheck, SonnetDBHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, SonnetDBHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddSonnetDB(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override SonnetDBHealthCheck CreateHealthCheck(HttpClient client, SonnetDBHealthCheckOptions? options)
        => new(client, options);

    protected override SonnetDBHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
