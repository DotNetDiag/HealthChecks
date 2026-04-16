using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator;

internal sealed class NamespacedServiceWatcher : IDisposable
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;
    private readonly OperatorDiagnostics _diagnostics;
    private readonly NotificationHandler _notificationHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Dictionary<HealthCheckResource, CancellationTokenSource> _watchers = new();

    public NamespacedServiceWatcher(
        IKubernetes client,
        ILogger<K8sOperator> logger,
        OperatorDiagnostics diagnostics,
        NotificationHandler notificationHandler,
        IHttpClientFactory httpClientFactory)
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
        _diagnostics = Guard.ThrowIfNull(diagnostics);
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        _notificationHandler = Guard.ThrowIfNull(notificationHandler);
    }

    internal Task Watch(HealthCheckResource resource, CancellationToken token)
    {
        Func<HealthCheckResource, bool> filter = k => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;

        if (!_watchers.Keys.Any(filter))
        {
            var watcherCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _ = WatchServicesAsync(resource, watcherCts.Token);

            _diagnostics.ServiceWatcherStarting(resource.Metadata.NamespaceProperty);

            _watchers.Add(resource, watcherCts);
        }

        return Task.CompletedTask;
    }

    internal void Stopwatch(HealthCheckResource resource)
    {
        Func<HealthCheckResource, bool> filter = (k) => k.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty;
        if (_watchers.Keys.Any(filter))
        {
            var svcResource = _watchers.Keys.FirstOrDefault(filter);
            if (svcResource != null)
            {
                _diagnostics.ServiceWatcherStopped(resource.Metadata.NamespaceProperty);
                _watchers[svcResource].Cancel();
                _watchers[svcResource].Dispose();
                _watchers.Remove(svcResource);
            }
        }
    }

    internal async Task OnServiceDiscoveredAsync(WatchEventType type, V1Service service, HealthCheckResource resource)
    {
        var uiService = await _client.ListNamespacedOwnedServiceAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
        var secret = await _client.ListNamespacedOwnedSecretAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);

        if (!service.Metadata.Labels.ContainsKey(resource.Spec.ServicesLabel))
        {
            type = WatchEventType.Deleted;
        }

        await HealthChecksPushService.PushNotification(
            type,
            resource,
            uiService!, // TODO: check
            service,
            secret!, // TODO: check
            _logger,
            _httpClientFactory);
    }

    public void Dispose()
    {
        _watchers.Values.ToList().ForEach(w =>
        {
            w.Cancel();
            w.Dispose();
        });

        _watchers.Clear();
    }

    private async Task WatchServicesAsync(HealthCheckResource resource, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var response = _client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(
                namespaceParameter: resource.Metadata.NamespaceProperty,
                labelSelector: $"{resource.Spec.ServicesLabel}",
                watch: true,
                cancellationToken: token);

            try
            {
#pragma warning disable CS0618 // KubernetesClient 19 exposes WatchAsync via an obsolete extension with no non-obsolete alternative yet
                await foreach (var (type, item) in response.WatchAsync<V1Service, V1ServiceList>(
                    onError: _diagnostics.ServiceWatcherThrow,
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
