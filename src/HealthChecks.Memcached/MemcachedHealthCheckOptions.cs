using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;

namespace HealthChecks.Memcached;

/// <summary>
/// Represents settings used by <see cref="MemcachedHealthCheck"/>.
/// </summary>
public sealed class MemcachedHealthCheckOptions
{
    /// <summary>
    /// The default Memcached port.
    /// </summary>
    public const int DEFAULT_PORT = 11211;

    /// <summary>
    /// Gets the underlying Enyim Memcached client options.
    /// </summary>
    public MemcachedClientOptions ClientOptions { get; } = new()
    {
        Protocol = MemcachedProtocol.Text,
        SocketPool = new SocketPoolOptions()
    };

    /// <summary>
    /// Gets or sets the expiration used for the temporary cache item.
    /// </summary>
    public TimeSpan CacheItemExpiration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the prefix used for the temporary cache item key.
    /// </summary>
    public string KeyPrefix { get; set; } = "healthchecks_memcached_";

    /// <summary>
    /// Adds a Memcached server to the health check client options.
    /// </summary>
    /// <param name="address">The Memcached server address.</param>
    /// <param name="port">The Memcached server port.</param>
    /// <returns>The current <see cref="MemcachedHealthCheckOptions"/> instance.</returns>
    public MemcachedHealthCheckOptions AddServer(string address, int port = DEFAULT_PORT)
    {
        Guard.ThrowIfNull(address, throwOnEmptyString: true);

        if (port is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        ClientOptions.AddServer(address, port);

        return this;
    }

    internal string CreateKey() => $"{KeyPrefix}{Guid.NewGuid():N}";
}
