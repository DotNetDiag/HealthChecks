using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;

using MicrosoftNullLoggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory;

namespace HealthChecks.Memcached.Tests;

internal static class MemcachedClientFactory
{
    public static MemcachedClient Create(string host, int port, TimeSpan? timeout = default)
    {
        var options = new MemcachedClientOptions
        {
            Protocol = MemcachedProtocol.Text,
            SocketPool = new SocketPoolOptions()
        };
        options.AddServer(host, port);

        if (timeout.HasValue)
        {
            options.SocketPool.ConnectionTimeout = timeout.Value;
            options.SocketPool.ReceiveTimeout = timeout.Value;
            options.SocketPool.QueueTimeout = timeout.Value;
        }

        return new MemcachedClient(MicrosoftNullLoggerFactory.Instance, new MemcachedClientConfigurationAdapter(options, MicrosoftNullLoggerFactory.Instance));
    }
}
