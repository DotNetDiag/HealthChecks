using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Operator;

internal sealed class ClusterServiceWatcher : IDisposable
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;
    private readonly OperatorDiagnostics _diagnostics;
    private readonly NotificationHandler _notificationHandler;
    private CancellationTokenSource? _watcherCts;

    public ClusterServiceWatcher(
      IKubernetes client,
      ILogger<K8sOperator> logger,
      OperatorDiagnostics diagnostics,
      NotificationHandler notificationHandler
      )
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
        _diagnostics = Guard.ThrowIfNull(diagnostics);
        _notificationHandler = Guard.ThrowIfNull(notificationHandler);
    }

    internal Task Watch(HealthCheckResource resource, CancellationToken token)
    {
        if (_watcherCts is { IsCancellationRequested: false })
        {
            return Task.CompletedTask;
        }

        _watcherCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _ = WatchServicesAsync(resource, _watcherCts.Token);

        _diagnostics.ServiceWatcherStarting("All");

        return Task.CompletedTask;
    }

    internal void Stopwatch(/*HealthCheckResource resource*/)
    {
        Dispose();
    }

    public void Dispose()
    {
        _watcherCts?.Cancel();
        _watcherCts?.Dispose();
        _watcherCts = null;
    }

    private async Task WatchServicesAsync(HealthCheckResource resource, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var response = _client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(
                labelSelector: $"{resource.Spec.ServicesLabel}",
                watch: true,
                cancellationToken: token);

            try
            {
#pragma warning disable CS0618 // KubernetesClient 19 exposes WatchAsync via an obsolete extension with no non-obsolete alternative yet
                await foreach (var (type, item) in response.WatchAsync<V1Service, V1ServiceList>(
                    onError: e =>
                    {
                        _diagnostics.ServiceWatcherThrow(e);
                        _logger.LogError(e, "Error watching cluster services");
                    },
                    cancellationToken: token))
                {
                    await _notificationHandler.NotifyDiscoveredServiceAsync(type, item, resource);
                }
#pragma warning restore CS0618
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
