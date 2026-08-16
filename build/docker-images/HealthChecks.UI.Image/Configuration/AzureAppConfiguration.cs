using HealthChecks.UI.Image.Configuration.Helpers;

namespace HealthChecks.UI.Image.Configuration;

public class AzureAppConfiguration
{
    public static bool Enabled
    {
        get
        {
            return EnvironmentVariable.HasValue(AzureAppConfigurationKeys.ENABLED) &&
                bool.TryParse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.ENABLED), out bool enabled) &&
                enabled;
        }
    }

    public static bool UseConnectionString =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.CONNECTION_STRING);

    public static bool UseLabel =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.LABEL);

    public static bool UseCacheExpiration =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.CACHE_EXPIRATION)
        && double.TryParse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.CACHE_EXPIRATION), out var _);

    public static double CacheExpiration => double.Parse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.CACHE_EXPIRATION)!);

    public static string? ConnectionString =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.CONNECTION_STRING);

    public static string? ManagedIdentityEndpoint =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.MANAGED_IDENTITY_ENDPOINT);

    public static string? Label =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.LABEL);
}
