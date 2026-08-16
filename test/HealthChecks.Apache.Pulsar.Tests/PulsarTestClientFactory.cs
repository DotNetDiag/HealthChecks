using System.Buffers;
using DotPulsar;
using DotPulsar.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.Apache.Pulsar.Tests;

internal static class PulsarTestClientFactory
{
    public static IPulsarClient CreateUnavailableClient()
    {
        IPulsarClient client = Substitute.For<IPulsarClient>();
        client
            .CreateProducer(Arg.Any<ProducerOptions<ReadOnlySequence<byte>>>())
            .Throws(new InvalidOperationException("Apache Pulsar broker is unavailable."));

        return client;
    }
}
