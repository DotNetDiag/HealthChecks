extern alias Operator;

using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Operator::HealthChecks.UI.K8s.Operator;
using Operator::HealthChecks.UI.K8s.Operator.Configuration;
using Operator::HealthChecks.UI.K8s.Operator.Crd;
using Operator::HealthChecks.UI.K8s.Operator.Diagnostics;
using Operator::HealthChecks.UI.K8s.Operator.Handlers;

namespace HealthChecks.UI.Tests;

public class k8s_operator_deployment_handler_should
{
    [Fact]
    public void use_the_configured_default_ui_image()
    {
        var handler = CreateHandler(new OperatorOptions
        {
            DefaultUIImage = "registry.example/healthchecks-ui:test"
        });

        var deployment = handler.Build(CreateResource());

        var container = deployment.Spec.Template.Spec.Containers.Single();
        container.Image.ShouldBe("registry.example/healthchecks-ui:test");
    }

    [Fact]
    public void add_runtime_hardening_defaults()
    {
        var handler = CreateHandler();

        var deployment = handler.Build(CreateResource());

        var container = deployment.Spec.Template.Spec.Containers.Single();
        container.Ports.Single().ContainerPort.ShouldBe(Constants.DEFAULT_PORT);
        container.LivenessProbe.HttpGet.Path.ShouldBe(Constants.DEFAULT_CONTAINER_HEALTH_PATH);
        container.ReadinessProbe.HttpGet.Path.ShouldBe(Constants.DEFAULT_CONTAINER_HEALTH_PATH);
        container.SecurityContext.AllowPrivilegeEscalation.ShouldBe(false);
        container.SecurityContext.RunAsNonRoot.ShouldBe(true);
        container.SecurityContext.Capabilities.Drop.ShouldContain("ALL");
    }

    [Fact]
    public void allow_probe_and_resource_overrides()
    {
        var handler = CreateHandler();
        var resource = CreateResource();
        resource.Spec.ReadinessProbe = new ProbeObject
        {
            Path = "/ready",
            InitialDelaySeconds = 1,
            PeriodSeconds = 2,
            TimeoutSeconds = 3,
            FailureThreshold = 4
        };
        resource.Spec.Resources = new ResourceRequirementsObject
        {
            Requests =
            {
                ["cpu"] = "100m"
            },
            Limits =
            {
                ["memory"] = "128Mi"
            }
        };

        var deployment = handler.Build(resource);

        var container = deployment.Spec.Template.Spec.Containers.Single();
        container.ReadinessProbe.HttpGet.Path.ShouldBe("/ready");
        container.ReadinessProbe.InitialDelaySeconds.ShouldBe(1);
        container.ReadinessProbe.PeriodSeconds.ShouldBe(2);
        container.ReadinessProbe.TimeoutSeconds.ShouldBe(3);
        container.ReadinessProbe.FailureThreshold.ShouldBe(4);
        container.Resources.Requests.ShouldContainKey("cpu");
        container.Resources.Limits.ShouldContainKey("memory");
    }

    private static DeploymentHandler CreateHandler(OperatorOptions? options = null)
    {
        return new DeploymentHandler(
            Substitute.For<IKubernetes>(),
            NullLogger<K8sOperator>.Instance,
            new OperatorDiagnostics(NullLoggerFactory.Instance),
            Options.Create(options ?? new OperatorOptions()));
    }

    private static HealthCheckResource CreateResource()
    {
        return new HealthCheckResource
        {
            Metadata = new V1ObjectMeta
            {
                Name = "healthchecks-ui",
                NamespaceProperty = "default",
                Uid = "uid"
            },
            Spec = new HealthCheckResourceSpec
            {
                Name = "healthchecks-ui"
            }
        };
    }
}
