namespace HealthChecks.UI.K8s.Operator;

public class HealthCheckResourceStatus
{
    public string? Phase { get; set; }
    public string? Message { get; set; }
    public long? ObservedGeneration { get; set; }
    public string? DeploymentName { get; set; }
    public string? ServiceName { get; set; }
    public int? AvailableReplicas { get; set; }
    public DateTimeOffset? LastTransitionTime { get; set; }
}
