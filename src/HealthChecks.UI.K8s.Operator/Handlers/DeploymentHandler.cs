using HealthChecks.UI.K8s.Operator.Configuration;
using HealthChecks.UI.K8s.Operator.Crd;
using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Extensions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static HealthChecks.UI.K8s.Operator.Constants;

namespace HealthChecks.UI.K8s.Operator.Handlers;

internal class DeploymentHandler
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;
    private readonly OperatorDiagnostics _operatorDiagnostics;
    private readonly OperatorOptions _options;

    public DeploymentHandler(IKubernetes client, ILogger<K8sOperator> logger, OperatorDiagnostics operatorDiagnostics, IOptions<OperatorOptions> options)
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
        _operatorDiagnostics = Guard.ThrowIfNull(operatorDiagnostics);
        _options = Guard.ThrowIfNull(options?.Value);
    }

    public Task<V1Deployment?> Get(HealthCheckResource resource)
    {
        return _client.ListNamespacedOwnedDeploymentAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
    }

    public async Task<V1Deployment> GetOrCreateAsync(HealthCheckResource resource)
    {
        var deployment = await Get(resource);
        if (deployment != null)
            return deployment;

        try
        {
            var deploymentResource = Build(resource);
            var response = await _client.AppsV1.CreateNamespacedDeploymentWithHttpMessagesAsync(deploymentResource, resource.Metadata.NamespaceProperty);
            deployment = response.Body;

            _operatorDiagnostics.DeploymentCreated(deployment.Metadata.Name);
        }
        catch (Exception ex)
        {
            _operatorDiagnostics.DeploymentOperationError(deployment?.Metadata.Name!, Deployment.Operation.ADD, ex.Message);
        }

        return deployment!;
    }

    public async Task DeleteAsync(HealthCheckResource resource)
    {
        try
        {
            await _client.AppsV1.DeleteNamespacedDeploymentAsync($"{resource.Spec.Name}-deploy",
                resource.Metadata.NamespaceProperty);
        }
        catch (Exception ex)
        {
            _operatorDiagnostics.DeploymentOperationError(resource.Spec.Name, Deployment.Operation.DELETE, ex.Message);
        }
    }

    public V1Deployment Build(HealthCheckResource resource)
    {
        var metadata = new V1ObjectMeta
        {
            OwnerReferences = new List<V1OwnerReference> {
                resource.CreateOwnerReference()
            },
            Annotations = new Dictionary<string, string>(),
            Labels = new Dictionary<string, string>
            {
                ["app"] = resource.Spec.Name
            },
            Name = $"{resource.Spec.Name}-deploy",
            NamespaceProperty = resource.Metadata.NamespaceProperty
        };

        var uiContainer = new V1Container
        {
            ImagePullPolicy = resource.Spec.ImagePullPolicy ?? Constants.DEFAULT_PULL_POLICY,
            Name = Constants.POD_NAME,
            Image = resource.Spec.Image ?? GetDefaultUIImage(),
            Ports = new List<V1ContainerPort>
            {
                new()
                {
                    ContainerPort = Constants.DEFAULT_PORT
                }
            },
            LivenessProbe = CreateHttpProbe(resource.Spec.LivenessProbe, defaultInitialDelaySeconds: 15),
            ReadinessProbe = CreateHttpProbe(resource.Spec.ReadinessProbe, defaultInitialDelaySeconds: 5),
            Resources = CreateResourceRequirements(resource.Spec.Resources),
            SecurityContext = CreateSecurityContext(),
            Env = new List<V1EnvVar>
            {
                ContainerExtensions.CreateEnvVar("enable_push_endpoint", "true"),
                ContainerExtensions.CreateEnvVar("push_endpoint_secret", valueFrom: new V1EnvVarSource
                {
                    SecretKeyRef = new V1SecretKeySelector
                    {
                        Key = "key",
                        Name = $"{resource.Spec.Name}-secret"
                    }
                }),
                ContainerExtensions.CreateEnvVar("Logging__LogLevel__Default", "Debug"),
                ContainerExtensions.CreateEnvVar("Logging__LogLevel__Microsoft", "Warning"),
                ContainerExtensions.CreateEnvVar("Logging__LogLevel__System", "Warning"),
                ContainerExtensions.CreateEnvVar("Logging__LogLevel__HealthChecks", "Information")
            }
        };

        uiContainer.MapCustomUIPaths(resource, _operatorDiagnostics);

        var tolerations = resource.Spec.Tolerations?.Select(toleration => new V1Toleration
        {
            Effect = toleration.Effect,
            Key = toleration.Key,
            OperatorProperty = toleration.Operator,
            TolerationSeconds = toleration.Seconds,
            Value = toleration.Value
        }).ToList() ?? new List<V1Toleration>();

        var spec = new V1DeploymentSpec
        {
            Selector = new V1LabelSelector
            {
                MatchLabels = new Dictionary<string, string>
                {
                    ["app"] = resource.Spec.Name
                }
            },
            Replicas = 1,
            Template = new V1PodTemplateSpec
            {
                Metadata = new V1ObjectMeta
                {
                    Labels = new Dictionary<string, string>
                    {
                        ["app"] = resource.Spec.Name
                    },
                },
                Spec = new V1PodSpec
                {
                    Containers = new List<V1Container>
                    {
                       uiContainer
                    },
                    Tolerations = tolerations
                }
            }
        };

        foreach (var annotation in resource.Spec.DeploymentAnnotations)
        {
            _logger.LogInformation("Adding annotation {Annotation} to ui deployment with value {AnnotationValue}", annotation.Name, annotation.Value);
            metadata.Annotations.Add(annotation.Name, annotation.Value);
        }

        var specification = spec.Template.Spec;
        var container = specification.Containers.First();

        for (int i = 0; i < resource.Spec.Webhooks.Count; i++)
        {
            var webhook = resource.Spec.Webhooks[i];
            _logger.LogInformation("Adding webhook configuration for webhook {Webhook}", webhook.Name);

            container.Env.Add(ContainerExtensions.CreateEnvVar($"HealthChecksUI__Webhooks__{i}__Name", webhook.Name));
            container.Env.Add(ContainerExtensions.CreateEnvVar($"HealthChecksUI__Webhooks__{i}__Uri", webhook.Uri));
            container.Env.Add(ContainerExtensions.CreateEnvVar($"HealthChecksUI__Webhooks__{i}__Payload", webhook.Payload));
            container.Env.Add(ContainerExtensions.CreateEnvVar($"HealthChecksUI__Webhooks__{i}__RestoredPayload", webhook.RestoredPayload));
        }

        if (resource.HasBrandingConfigured())
        {
            const string volumeName = "healthchecks-volume";

            specification.Volumes ??= new List<V1Volume>();
            container.VolumeMounts ??= new List<V1VolumeMount>();

            specification.Volumes.Add(new V1Volume
            {
                Name = volumeName,
                ConfigMap = new V1ConfigMapVolumeSource
                {
                    Name = $"{resource.Spec.Name}-config"
                }
            });

            container.Env.Add(ContainerExtensions.CreateEnvVar("ui_stylesheet", $"{Constants.STYLES_PATH}/{Constants.STYLE_SHEET_NAME}"));
            container.VolumeMounts.Add(new V1VolumeMount
            {
                MountPath = $"/app/{Constants.STYLES_PATH}",
                Name = volumeName
            });
        }

        return new V1Deployment
        {
            Metadata = metadata,
            Spec = spec
        };
    }

    private string GetDefaultUIImage()
    {
        return string.IsNullOrWhiteSpace(_options.DefaultUIImage)
            ? Constants.IMAGE_NAME
            : _options.DefaultUIImage;
    }

    private static V1Probe CreateHttpProbe(ProbeObject? probe, int defaultInitialDelaySeconds)
    {
        return new V1Probe
        {
            HttpGet = new V1HTTPGetAction
            {
                Path = probe?.Path ?? Constants.DEFAULT_CONTAINER_HEALTH_PATH,
                Port = Constants.DEFAULT_PORT,
                Scheme = "HTTP"
            },
            InitialDelaySeconds = probe?.InitialDelaySeconds ?? defaultInitialDelaySeconds,
            PeriodSeconds = probe?.PeriodSeconds ?? 10,
            TimeoutSeconds = probe?.TimeoutSeconds ?? 2,
            FailureThreshold = probe?.FailureThreshold ?? 3
        };
    }

    private static V1ResourceRequirements? CreateResourceRequirements(ResourceRequirementsObject? resources)
    {
        if (resources == null)
        {
            return null;
        }

        return new V1ResourceRequirements
        {
            Limits = CreateResourceQuantityMap(resources.Limits),
            Requests = CreateResourceQuantityMap(resources.Requests)
        };
    }

    private static Dictionary<string, ResourceQuantity>? CreateResourceQuantityMap(Dictionary<string, string> resources)
    {
        return resources.Count == 0
            ? null
            : resources.ToDictionary(pair => pair.Key, pair => new ResourceQuantity(pair.Value));
    }

    private static V1SecurityContext CreateSecurityContext()
    {
        return new V1SecurityContext
        {
            AllowPrivilegeEscalation = false,
            Capabilities = new V1Capabilities
            {
                Drop = new List<string> { "ALL" }
            },
            RunAsNonRoot = true
        };
    }
}
