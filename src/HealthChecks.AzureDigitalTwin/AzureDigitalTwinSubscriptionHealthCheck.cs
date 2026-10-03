using Azure.Core;
using Azure.ResourceManager.DigitalTwins;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureDigitalTwin;

public class AzureDigitalTwinSubscriptionHealthCheck : AzureDigitalTwinHealthCheck, IHealthCheck
{
    private readonly ResourceIdentifier _resourceId;

    public AzureDigitalTwinSubscriptionHealthCheck(string clientId, string clientSecret, string tenantId, string resourceId)
        : base(clientId, clientSecret, tenantId)
    {
        _resourceId = ParseResourceId(resourceId);
    }

    public AzureDigitalTwinSubscriptionHealthCheck(TokenCredential tokenCredential, string resourceId)
        : base(tokenCredential)
    {
        _resourceId = ParseResourceId(resourceId);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = ManagementClientConnections.GetOrAdd(ClientConnectionKey, _ => CreateManagementClient());
            var resource = managementClient.GetDigitalTwinsDescriptionResource(_resourceId);
            _ = await resource.GetAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static ResourceIdentifier ParseResourceId(string resourceId)
    {
        var identifier = new ResourceIdentifier(Guard.ThrowIfNull(resourceId, true));
        if (identifier.ResourceType != DigitalTwinsDescriptionResource.ResourceType
            || identifier.SubscriptionId is null
            || identifier.ResourceGroupName is null)
        {
            throw new ArgumentException("The resource id must identify an Azure Digital Twins instance in a subscription and resource group.", nameof(resourceId));
        }

        return identifier;
    }
}
