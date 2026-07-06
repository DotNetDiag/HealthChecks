using System.Buffers;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Apache.Pulsar;

/// <summary>
/// A health check for Apache Pulsar brokers.
/// </summary>
public sealed class PulsarHealthCheck : IHealthCheck
{
    private readonly IPulsarClient _client;
    private readonly PulsarHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="PulsarHealthCheck"/>.
    /// </summary>
    /// <param name="client">The Pulsar client used to verify broker connectivity.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public PulsarHealthCheck(IPulsarClient client, PulsarHealthCheckOptions? options = default)
    {
        _client = Guard.ThrowIfNull(client);
        _options = options ?? new PulsarHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateOptions();

            await using IProducer<ReadOnlySequence<byte>> producer = _client
                .NewProducer()
                .Topic(_options.Topic)
                .Create();

            await producer.Send(_options.MessageBuilder(_options), cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Topic))
        {
            throw new InvalidOperationException($"{nameof(PulsarHealthCheckOptions.Topic)} must be configured.");
        }

        if (_options.MessageBuilder is null)
        {
            throw new InvalidOperationException($"{nameof(PulsarHealthCheckOptions.MessageBuilder)} must be configured.");
        }
    }
}
