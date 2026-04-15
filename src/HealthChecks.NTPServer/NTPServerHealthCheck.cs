using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.NTPServer;

/// <summary>
/// A health check that queries an NTP server to verify time synchronization.
/// </summary>
public class NTPServerHealthCheck : IHealthCheck
{
    private const int NTP_DATA_LENGTH = 48;
    private static readonly DateTime _ntpEpoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly NTPServerHealthCheckOptions _options;

    public NTPServerHealthCheck(NTPServerHealthCheckOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var offset = await GetTimeOffsetAsync(_options.NtpServer, _options.NtpPort, cancellationToken).ConfigureAwait(false);
            var absOffset = Math.Abs(offset);

            if (absOffset <= _options.ToleranceSeconds)
                return HealthCheckResult.Healthy();

            if (absOffset <= _options.ToleranceSeconds * 2)
                return HealthCheckResult.Degraded($"NTP time offset of {offset:F2}s exceeds tolerance of {_options.ToleranceSeconds}s.");

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"NTP time offset of {offset:F2}s exceeds maximum tolerance of {_options.ToleranceSeconds * 2}s.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }

    private static async Task<double> GetTimeOffsetAsync(string ntpServer, int ntpPort, CancellationToken cancellationToken)
    {
        var addresses = await Task.Run(() => Dns.GetHostAddresses(ntpServer), cancellationToken).ConfigureAwait(false);
        if (addresses.Length == 0)
            throw new InvalidOperationException($"Could not resolve NTP server host: {ntpServer}");

        List<Exception> failures = [];

        foreach (var address in addresses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await GetTimeOffsetAsync(address, ntpPort, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failures.Add(new InvalidOperationException(
                    $"Failed to query NTP server '{ntpServer}' using address '{address}' on port {ntpPort}.",
                    ex));
            }
        }

        throw new AggregateException($"Failed to query NTP server '{ntpServer}' on port {ntpPort}.", failures);
    }

    private static async Task<double> GetTimeOffsetAsync(IPAddress address, int ntpPort, CancellationToken cancellationToken)
    {
        var endpoint = new IPEndPoint(address, ntpPort);
        using var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

        var ntpData = new byte[NTP_DATA_LENGTH];
        ntpData[0] = 0x1B; // LI=0, VN=3, Mode=3 (client)

        await socket.SendAsync(ntpData, SocketFlags.None, cancellationToken).ConfigureAwait(false);

        var received = await socket.ReceiveAsync(ntpData, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        if (received < NTP_DATA_LENGTH)
            throw new InvalidOperationException("Invalid NTP response received.");

        var ntpTime = ExtractNtpTimestamp(ntpData, 40);
        var localTime = DateTime.UtcNow;
        return (ntpTime - localTime).TotalSeconds;
    }

    private static DateTime ExtractNtpTimestamp(byte[] data, int offset)
    {
        ulong intPart = ((ulong)data[offset] << 24) | ((ulong)data[offset + 1] << 16) |
                        ((ulong)data[offset + 2] << 8) | data[offset + 3];
        ulong fracPart = ((ulong)data[offset + 4] << 24) | ((ulong)data[offset + 5] << 16) |
                         ((ulong)data[offset + 6] << 8) | data[offset + 7];

        var milliseconds = (intPart * 1000) + (fracPart * 1000 / 0x100000000L);
        return _ntpEpoch.AddMilliseconds(milliseconds);
    }
}
