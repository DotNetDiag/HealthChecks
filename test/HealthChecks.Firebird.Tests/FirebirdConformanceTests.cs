namespace HealthChecks.Firebird.Tests;

public class FirebirdConformanceTests : ConformanceTests<string, FirebirdHealthCheck, FirebirdHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, string>? clientFactory = default,
        Func<IServiceProvider, FirebirdHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddFirebird(
            sp =>
            {
                FirebirdHealthCheckOptions options = optionsFactory?.Invoke(sp) ?? new FirebirdHealthCheckOptions();
                options.ConnectionString = clientFactory?.Invoke(sp) ?? sp.GetRequiredService<string>();

                return options;
            },
            healthCheckName,
            failureStatus,
            tags,
            timeout);
    }

    protected override string CreateClientForNonExistingEndpoint()
        => "Database=127.0.0.1/1:healthchecks.fdb;User=SYSDBA;Password=masterkey;Connection Timeout=1";

    protected override FirebirdHealthCheck CreateHealthCheck(string client, FirebirdHealthCheckOptions? options)
    {
        FirebirdHealthCheckOptions healthCheckOptions = options ?? CreateHealthCheckOptions();
        healthCheckOptions.ConnectionString = client;

        return new FirebirdHealthCheck(healthCheckOptions);
    }

    protected override FirebirdHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
