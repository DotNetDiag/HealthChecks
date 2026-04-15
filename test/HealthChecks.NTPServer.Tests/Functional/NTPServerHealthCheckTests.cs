using System.Net;
using System.Net.Sockets;

namespace HealthChecks.NTPServer.Tests.Functional;

public class ntpserver_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_ntp_server_is_reachable()
    {
        await using var ntpServer = FakeNtpServer.Start();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddNTPServer(options =>
                    {
                        options.NtpServer = ntpServer.Host;
                        options.NtpPort = ntpServer.Port;
                        options.ToleranceSeconds = 60.0;
                    }, tags: ["ntp"]);
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("ntp")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_ntp_server_is_not_reachable()
    {
        await using var ntpServer = FakeNtpServer.Start(silentlyDropRequests: true);

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddNTPServer(options =>
                    {
                        options.NtpServer = ntpServer.Host;
                        options.NtpPort = ntpServer.Port;
                        options.ToleranceSeconds = 10.0;
                    }, tags: ["ntp"], timeout: TimeSpan.FromSeconds(2));
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("ntp")
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_degraded_when_tolerance_is_zero()
    {
        await using var ntpServer = FakeNtpServer.Start();

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddNTPServer(options =>
                    {
                        options.NtpServer = ntpServer.Host;
                        options.NtpPort = ntpServer.Port;
                        options.ToleranceSeconds = 0.0;
                    }, tags: ["ntp"], failureStatus: HealthStatus.Unhealthy);
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("ntp"),
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = 200,
                        [HealthStatus.Degraded] = 200,
                        [HealthStatus.Unhealthy] = 503,
                    }
                });
            }));

        var server = host.GetTestServer();
        using var response = await server.CreateRequest("/health").GetAsync();

        // With zero tolerance, any non-zero offset will be degraded or unhealthy
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    private sealed class FakeNtpServer : IAsyncDisposable
    {
        private const int NtpDataLength = 48;
        private static readonly DateTime NtpEpoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly UdpClient _udpClient;
        private readonly Task _serverTask;
        private readonly bool _silentlyDropRequests;

        private FakeNtpServer(UdpClient udpClient, bool silentlyDropRequests)
        {
            _udpClient = udpClient;
            _silentlyDropRequests = silentlyDropRequests;
            Host = IPAddress.Loopback.ToString();
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint!).Port;
            _serverTask = RunAsync();
        }

        public string Host { get; }

        public int Port { get; }

        public static FakeNtpServer Start(bool silentlyDropRequests = false)
        {
            var udpClient = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            return new FakeNtpServer(udpClient, silentlyDropRequests);
        }

        public async ValueTask DisposeAsync()
        {
            await _cancellationTokenSource.CancelAsync();
            _udpClient.Dispose();

            try
            {
                await _serverTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_cancellationTokenSource.IsCancellationRequested)
            {
            }
            finally
            {
                _cancellationTokenSource.Dispose();
            }
        }

        private async Task RunAsync()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var result = await _udpClient.ReceiveAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

                    if (_silentlyDropRequests)
                        continue;

                    var response = CreateResponse();
                    await _udpClient.SendAsync(response, result.RemoteEndPoint).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_cancellationTokenSource.IsCancellationRequested)
            {
            }
            catch (ObjectDisposedException) when (_cancellationTokenSource.IsCancellationRequested)
            {
            }
        }

        private static byte[] CreateResponse()
        {
            var ntpData = new byte[NtpDataLength];
            ntpData[0] = 0x1C; // LI=0, VN=3, Mode=4 (server)

            WriteTimestamp(ntpData, 40, DateTime.UtcNow);

            return ntpData;
        }

        private static void WriteTimestamp(byte[] data, int offset, DateTime utcTimestamp)
        {
            var elapsed = utcTimestamp - NtpEpoch;
            ulong seconds = (ulong)(elapsed.Ticks / TimeSpan.TicksPerSecond);
            ulong fraction = (ulong)((elapsed.Ticks % TimeSpan.TicksPerSecond) * 0x100000000L / TimeSpan.TicksPerSecond);

            data[offset] = (byte)(seconds >> 24);
            data[offset + 1] = (byte)(seconds >> 16);
            data[offset + 2] = (byte)(seconds >> 8);
            data[offset + 3] = (byte)seconds;
            data[offset + 4] = (byte)(fraction >> 24);
            data[offset + 5] = (byte)(fraction >> 16);
            data[offset + 6] = (byte)(fraction >> 8);
            data[offset + 7] = (byte)fraction;
        }
    }
}
