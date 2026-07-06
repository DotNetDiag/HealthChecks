using System.Net;
#if NET8_0_OR_GREATER
using System.Net.Security;
#endif
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol.Binary;
using Enyim.Caching.Memcached.Protocol.Text;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Memcached;

internal sealed class MemcachedClientConfigurationAdapter : IMemcachedClientConfiguration
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly MemcachedClientOptions _options;

    public MemcachedClientConfigurationAdapter(MemcachedClientOptions options, ILoggerFactory loggerFactory)
    {
        _options = Guard.ThrowIfNull(options);
        _loggerFactory = Guard.ThrowIfNull(loggerFactory);
        _options.SocketPool ??= new SocketPoolOptions();

        Servers = _options.Servers
            .Select(server => (EndPoint)new DnsEndPoint(server.Address, server.Port))
            .ToList();
        SocketPool = new SocketPoolConfigurationAdapter(_options.SocketPool);

        var authentication = _options.Authentication;
        Authentication = authentication is null || string.IsNullOrWhiteSpace(authentication.Type)
            ? null!
            : new AuthenticationConfigurationAdapter(authentication);
    }

    public IList<EndPoint> Servers { get; }

    public ISocketPoolConfiguration SocketPool { get; }

    public IAuthenticationConfiguration Authentication { get; }

    public bool UseSslStream => _options.UseSslStream;

    public bool SuppressException => _options.SuppressException;

#if NET8_0_OR_GREATER
    public bool UseIPv6 => _options.UseIPv6;

    public SslClientAuthenticationOptions SslClientAuth => _options.SslClientAuth;
#endif

    public IMemcachedKeyTransformer CreateKeyTransformer()
    {
        return CreateConfiguredInstance(_options.KeyTransformer, static () => new DefaultKeyTransformer());
    }

    public IMemcachedNodeLocator CreateNodeLocator()
    {
        if (_options.NodeLocatorFactory is not null)
        {
            return _options.NodeLocatorFactory.Create();
        }

#if NET8_0_OR_GREATER
        if (_options.UseLegacyNodeLocator)
        {
            return new LegacyNodeLocator();
        }
#endif

        return new DefaultNodeLocator();
    }

    public ITranscoder CreateTranscoder()
    {
        return CreateConfiguredInstance(_options.Transcoder, static () => new DefaultTranscoder());
    }

    public IServerPool CreatePool()
    {
        ILogger logger = _loggerFactory.CreateLogger<MemcachedClientConfigurationAdapter>();

        return _options.Protocol == MemcachedProtocol.Binary
            ? new BinaryPool(this, logger)
            : new DefaultServerPool(this, new TextOperationFactory(), logger);
    }

    private static T CreateConfiguredInstance<T>(string? typeName, Func<T> defaultFactory)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return defaultFactory();
        }

        Type type = Type.GetType(typeName, throwOnError: true)!;
        return (T)Activator.CreateInstance(type)!;
    }

    private sealed class SocketPoolConfigurationAdapter : ISocketPoolConfiguration
    {
        private readonly SocketPoolOptions _options;

        public SocketPoolConfigurationAdapter(SocketPoolOptions options)
        {
            _options = Guard.ThrowIfNull(options);
        }

        public int MinPoolSize
        {
            get => _options.MinPoolSize;
            set => _options.MinPoolSize = value;
        }

        public int MaxPoolSize
        {
            get => _options.MaxPoolSize;
            set => _options.MaxPoolSize = value;
        }

        public TimeSpan ConnectionTimeout
        {
            get => _options.ConnectionTimeout;
            set => _options.ConnectionTimeout = value;
        }

        public TimeSpan QueueTimeout
        {
            get => _options.QueueTimeout;
            set => _options.QueueTimeout = value;
        }

        public TimeSpan ReceiveTimeout
        {
            get => _options.ReceiveTimeout;
            set => _options.ReceiveTimeout = value;
        }

        public TimeSpan DeadTimeout
        {
            get => _options.DeadTimeout;
            set => _options.DeadTimeout = value;
        }

        public TimeSpan ConnectionIdleTimeout
        {
            get => _options.ConnectionIdleTimeout;
            set => _options.ConnectionIdleTimeout = value;
        }

        public TimeSpan InitPoolTimeout
        {
            get => _options.InitPoolTimeout;
            set => _options.InitPoolTimeout = value;
        }

        public INodeFailurePolicyFactory FailurePolicyFactory
        {
            get => _options.FailurePolicyFactory;
            set => _options.FailurePolicyFactory = value;
        }
    }

    private sealed class AuthenticationConfigurationAdapter : IAuthenticationConfiguration
    {
        private readonly Authentication _authentication;

        public AuthenticationConfigurationAdapter(Authentication authentication)
        {
            _authentication = Guard.ThrowIfNull(authentication);
            Type = System.Type.GetType(_authentication.Type, throwOnError: true)!;
        }

        public Type Type { get; set; }

        public Dictionary<string, object> Parameters => _authentication.Parameters.ToDictionary(
            parameter => parameter.Key,
            parameter => (object)parameter.Value);
    }
}
