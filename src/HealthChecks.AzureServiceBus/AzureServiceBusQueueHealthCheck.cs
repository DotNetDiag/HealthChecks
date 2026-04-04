using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueHealthCheckOptions>, IHealthCheck
{
    private readonly string _queueKey;

    public AzureServiceBusQueueHealthCheck(AzureServiceBusQueueHealthCheckOptions options, ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        Guard.ThrowIfNull(options.QueueName, true);

        _queueKey = $"{nameof(AzureServiceBusQueueHealthCheck)}_{ConnectionKey}_{Options.QueueName}";
    }

    public AzureServiceBusQueueHealthCheck(AzureServiceBusQueueHealthCheckOptions options)
        : this(options, new ServiceBusClientProvider())
    { }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Options.UsePeekMode)
                await CheckWithReceiver().ConfigureAwait(false);
            else
                await CheckWithManagement().ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

        async Task CheckWithReceiver()
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient()).ConfigureAwait(false);
            var receiver = await ClientCache.GetOrAddAsyncDisposableAsync(
                _queueKey,
                _ => client.CreateReceiver(Options.QueueName))
                .ConfigureAwait(false);

            // Some peek flows can outlive the requested cancellation, so race the SDK call against the token.
            // PR: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/2433
            // Issue: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2432
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, linkedTokenSource.Token);
            var peekTask = receiver.PeekMessageAsync(cancellationToken: cancellationToken);
            var completedTask = await Task.WhenAny(peekTask, cancellationTask).ConfigureAwait(false);

            if (completedTask == peekTask)
            {
                await linkedTokenSource.CancelAsync().ConfigureAwait(false);
            }

            await completedTask.ConfigureAwait(false);
        }

        Task CheckWithManagement()
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            return managementClient.GetQueueRuntimePropertiesAsync(Options.QueueName, cancellationToken);
        }
    }
}
