using HealthChecks.UI.K8s.Operator.Diagnostics;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator;

public static class ContainerExtensions
{
    internal static void MapCustomUIPaths(this V1Container container, HealthCheckResource resource, OperatorDiagnostics diagnostics)
    {
        var uiPath = resource.Spec.UiPath ?? Constants.DEFAULT_UI_PATH;
        container.Env.Add(CreateEnvVar("ui_path", uiPath));
        diagnostics.UiPathConfigured(nameof(resource.Spec.UiPath), uiPath);

        if (!string.IsNullOrEmpty(resource.Spec.UiApiPath))
        {
            container.Env.Add(CreateEnvVar("ui_api_path", resource.Spec.UiApiPath));
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiApiPath), resource.Spec.UiApiPath);
        }

        if (!string.IsNullOrEmpty(resource.Spec.UiResourcesPath))
        {
            container.Env.Add(CreateEnvVar("ui_resources_path", resource.Spec.UiResourcesPath));
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiResourcesPath), resource.Spec.UiResourcesPath);
        }

        if (!string.IsNullOrEmpty(resource.Spec.UiWebhooksPath))
        {
            container.Env.Add(CreateEnvVar("ui_webhooks_path", resource.Spec.UiWebhooksPath));
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiWebhooksPath), resource.Spec.UiWebhooksPath);
        }

        if (resource.Spec.UiNoRelativePaths.HasValue)
        {
            var noRelativePaths = resource.Spec.UiNoRelativePaths.Value.ToString();
            container.Env.Add(CreateEnvVar("ui_no_relative_paths", noRelativePaths));
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiNoRelativePaths), noRelativePaths);
        }
    }

    internal static V1EnvVar CreateEnvVar(string name, string? value = null, V1EnvVarSource? valueFrom = null)
        => new()
        {
            Name = name,
            Value = value,
            ValueFrom = valueFrom
        };
}
