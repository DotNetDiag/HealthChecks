using Enyim.Caching;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Memcached;

/// <summary>
/// A health check for Memcached services.
/// </summary>
public sealed class MemcachedHealthCheck : IHealthCheck
{
    private const int MAX_KEY_LENGTH = 250;

    private readonly IMemcachedClient? _memcachedClient;
    private readonly MemcachedHealthCheckOptions _options;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Creates a new instance of the Memcached health check.
    /// </summary>
    /// <param name="memcachedClient">The Memcached client used by the health check.</param>
    /// <param name="options">Optional settings used by the health check operation.</param>
    public MemcachedHealthCheck(IMemcachedClient memcachedClient, MemcachedHealthCheckOptions? options = default)
    {
        _memcachedClient = Guard.ThrowIfNull(memcachedClient);
        _options = options ?? new MemcachedHealthCheckOptions();
    }

    internal MemcachedHealthCheck(MemcachedHealthCheckOptions options, ILoggerFactory loggerFactory)
    {
        _options = Guard.ThrowIfNull(options);
        _loggerFactory = Guard.ThrowIfNull(loggerFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateOptions(requiresServerConfiguration: _memcachedClient is null);

            string key = _options.CreateKey();
            string expectedValue = Guid.NewGuid().ToString("N");

            cancellationToken.ThrowIfCancellationRequested();

            IMemcachedClient client = _memcachedClient ?? new MemcachedClient(
                _loggerFactory!,
                new MemcachedClientConfigurationAdapter(_options.ClientOptions, _loggerFactory!));

            try
            {
                bool stored = await client.SetAsync(key, expectedValue, _options.CacheItemExpiration).ConfigureAwait(false);
                if (!stored)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Memcached cache item could not be stored.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                string? actualValue = await client.GetValueAsync<string>(key).ConfigureAwait(false);
                if (!string.Equals(expectedValue, actualValue, StringComparison.Ordinal))
                {
                    await client.RemoveAsync(key).ConfigureAwait(false);

                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Memcached cache item could not be read back.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                bool removed = await client.RemoveAsync(key).ConfigureAwait(false);
                if (!removed)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Memcached cache item could not be removed.");
                }

                return HealthCheckResult.Healthy();
            }
            finally
            {
                if (_memcachedClient is null && client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private void ValidateOptions(bool requiresServerConfiguration)
    {
        if (_options.CacheItemExpiration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{nameof(MemcachedHealthCheckOptions.CacheItemExpiration)} must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(_options.KeyPrefix))
        {
            throw new InvalidOperationException($"{nameof(MemcachedHealthCheckOptions.KeyPrefix)} must be configured.");
        }

        if (_options.KeyPrefix.Length + 32 > MAX_KEY_LENGTH)
        {
            throw new InvalidOperationException($"{nameof(MemcachedHealthCheckOptions.KeyPrefix)} must leave enough room for a generated Memcached key.");
        }

        if (requiresServerConfiguration && _options.ClientOptions.Servers.Count == 0)
        {
            throw new InvalidOperationException("At least one Memcached server must be configured.");
        }
    }
}
