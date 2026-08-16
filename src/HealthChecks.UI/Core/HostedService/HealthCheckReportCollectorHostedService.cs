using HealthChecks.UI.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.HostedService;

internal class HealthCheckCollectorHostedService : IHostedService
{
    private readonly ILogger<HealthCheckCollectorHostedService> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ServerAddressesService _serverAddressesService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Settings _settings;

    private Task? _executingTask;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public HealthCheckCollectorHostedService
        (IServiceScopeFactory scopeFactory,
        IOptions<Settings> settings,
        ServerAddressesService serverAddressesService,
        ILogger<HealthCheckCollectorHostedService> logger,
        IHostApplicationLifetime lifetime)
    {
        _scopeFactory = Guard.ThrowIfNull(scopeFactory);
        _serverAddressesService = Guard.ThrowIfNull(serverAddressesService);
        _logger = Guard.ThrowIfNull(logger);
        _lifetime = Guard.ThrowIfNull(lifetime);
        _settings = settings.Value ?? new Settings();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_cancellationTokenSource.Token);

        return _executingTask.IsCompleted
            ? _executingTask
            : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();

        if (_executingTask != null)
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var applicationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using var startedRegistration = _lifetime.ApplicationStarted.Register(() => applicationStarted.TrySetResult());
        using var stoppingRegistration = _lifetime.ApplicationStopping.Register(() => applicationStarted.TrySetCanceled(cancellationToken));

        await applicationStarted.Task.ConfigureAwait(false);

        try
        {
            await CollectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // We are halting, task cancellation is expected.
        }
    }

    private async Task CollectAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Collect should not be triggered until IServerAddressFeature reports the listening endpoints

            _logger.LogDebug("Executing HealthCheck collector HostedService.");

            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IHealthCheckReportCollector>();
                    await runner.Collect(cancellationToken).ConfigureAwait(false);

                    _logger.LogDebug("HealthCheck collector HostedService executed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HealthCheck collector HostedService threw an error: {Error}", ex.Message);
                }
            }

            await Task.Delay(_settings.EvaluationTimeInSeconds * 1000, cancellationToken).ConfigureAwait(false);
        }
    }
}
