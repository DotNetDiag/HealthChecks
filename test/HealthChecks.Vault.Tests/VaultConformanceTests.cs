namespace HealthChecks.Vault.Tests;

public class VaultConformanceTests : ConformanceTests<HttpClient, VaultHealthCheck, VaultHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, HttpClient>? clientFactory = default,
        Func<IServiceProvider, VaultHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddVault(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);
    }

    protected override HttpClient CreateClientForNonExistingEndpoint()
        => new()
        {
            BaseAddress = new Uri("http://127.0.0.1:1")
        };

    protected override VaultHealthCheck CreateHealthCheck(HttpClient client, VaultHealthCheckOptions? options)
        => new(client, options);

    protected override VaultHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
