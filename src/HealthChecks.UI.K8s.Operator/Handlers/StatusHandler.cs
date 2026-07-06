using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Handlers;

internal class StatusHandler
{
    private const string FIELD_MANAGER = "healthchecks-ui-k8s-operator";

    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;

    public StatusHandler(IKubernetes client, ILogger<K8sOperator> logger)
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
    }

    public async Task PatchAsync(HealthCheckResource resource, HealthCheckResourceStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var patch = new V1Patch(CreatePatch(resource, status), V1Patch.PatchType.MergePatch);

            await _client.CustomObjects.PatchNamespacedCustomObjectStatusAsync(
                patch,
                Constants.GROUP,
                Constants.VERSION,
                resource.Metadata.NamespaceProperty,
                Constants.PLURAL,
                resource.Metadata.Name,
                dryRun: null,
                fieldManager: FIELD_MANAGER,
                fieldValidation: null,
                force: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not patch status for HealthCheck resource {name}", resource.Metadata.Name);
        }
    }

    private static object CreatePatch(HealthCheckResource resource, HealthCheckResourceStatus status)
    {
        return new
        {
            status = new
            {
                phase = status.Phase,
                message = status.Message,
                observedGeneration = status.ObservedGeneration ?? resource.Metadata.Generation,
                deploymentName = status.DeploymentName,
                serviceName = status.ServiceName,
                availableReplicas = status.AvailableReplicas,
                lastTransitionTime = status.LastTransitionTime ?? DateTimeOffset.UtcNow
            }
        };
    }
}
