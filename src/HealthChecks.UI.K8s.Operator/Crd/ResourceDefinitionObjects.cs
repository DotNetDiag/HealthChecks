namespace HealthChecks.UI.K8s.Operator.Crd;

public class NameValueObject
{
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class TolerationObject
{
    public string? Key { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public string? Effect { get; set; }
    public long? Seconds { get; set; }
}

public class ResourceRequirementsObject
{
    public Dictionary<string, string> Limits { get; set; } = new();
    public Dictionary<string, string> Requests { get; set; } = new();
}

public class ProbeObject
{
    public string? Path { get; set; }
    public int? InitialDelaySeconds { get; set; }
    public int? PeriodSeconds { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? FailureThreshold { get; set; }
}

public class WebHookObject
{
    public string? Name { get; set; }
    public string? Uri { get; set; }
    public string? Payload { get; set; }
    public string? RestoredPayload { get; set; }
}
