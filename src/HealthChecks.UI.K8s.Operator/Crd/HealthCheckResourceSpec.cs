using HealthChecks.UI.K8s.Operator.Crd;

namespace HealthChecks.UI.K8s.Operator;

public class HealthCheckResourceSpec
{
    public string Name { get; set; } = null!;
    public string Scope { get; set; } = Constants.Deployment.Scope.CLUSTER;
    public int? PortNumber { get; set; }
    public string? ServiceType { get; set; }
    public string? UiPath { get; set; }
    public string? UiApiPath { get; set; }
    public string? UiResourcesPath { get; set; }
    public string? UiWebhooksPath { get; set; }
    public bool? UiNoRelativePaths { get; set; }
    public string? ServicesLabel { get; set; } = "HealthChecks";
    public string HealthChecksPath { get; set; } = Constants.DEFAULT_HEALTH_PATH;
    public string HealthChecksScheme { get; set; } = Constants.DEFAULT_SCHEME;
    public string? Image { get; set; }
    public string? ImagePullPolicy { get; set; }
    public string StylesheetContent { get; set; } = string.Empty;
    public ProbeObject? LivenessProbe { get; set; }
    public ProbeObject? ReadinessProbe { get; set; }
    public ResourceRequirementsObject? Resources { get; set; }
    public List<NameValueObject> ServiceAnnotations { get; set; } = new List<NameValueObject>();
    public List<NameValueObject> DeploymentAnnotations { get; set; } = new List<NameValueObject>();
    public List<WebHookObject> Webhooks { get; set; } = new List<WebHookObject>();
    public List<TolerationObject> Tolerations { get; set; } = new List<TolerationObject>();
}
