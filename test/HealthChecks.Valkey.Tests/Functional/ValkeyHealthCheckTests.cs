using System.Net;
using StackExchange.Redis;
using StackExchange.Redis.Maintenance;
using StackExchange.Redis.Profiling;

namespace HealthChecks.Valkey.Tests.Functional;

public class valkey_healthcheck_should(ValkeyContainerFixture valkeyContainerFixture) : IClassFixture<ValkeyContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_valkey_is_available_with_connection_string()
    {
        var connectionString = $"{valkeyContainerFixture.GetConnectionString()},allowAdmin=true";

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddValkey(connectionString, tags: ["valkey"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_valkey_are_available_with_connection_string()
    {
        var connectionString = $"{valkeyContainerFixture.GetConnectionString()},allowAdmin=true";

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddValkey(connectionString, tags: ["valkey"], name: "1")
                    .AddValkey(connectionString, tags: ["valkey"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_valkey_is_available_with_connection_multiplexer()
    {
        var connectionString = $"{valkeyContainerFixture.GetConnectionString()},allowAdmin=true";

        using var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddValkey(connectionMultiplexer, tags: ["valkey"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_multiple_valkey_are_available_with_connection_multiplexer()
    {
        var connectionString = $"{valkeyContainerFixture.GetConnectionString()},allowAdmin=true";

        using var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);

        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

                services.AddHealthChecks()
                    .AddValkey(connectionMultiplexer, tags: ["valkey"], name: "1")
                    .AddValkey(sp => sp.GetRequiredService<IConnectionMultiplexer>(), tags: ["valkey"], name: "2");
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_connection_multiplexer_factory_throws_on_connect()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConnectionMultiplexer>(
                    _ => ConnectionMultiplexer.Connect("nonexistinghost:6379,allowAdmin=true"));
                services.AddHealthChecks()
                    .AddValkey(serviceProvider => serviceProvider.GetRequiredService<IConnectionMultiplexer>());
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_valkey_is_not_available()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddValkey("nonexistinghost:6379,allowAdmin=true", tags: ["valkey"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey")
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_valkey_is_not_available_within_specified_timeout()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddValkey("nonexistinghost:6379,allowAdmin=true,connectRetry=2147483647", tags: ["valkey"], timeout: TimeSpan.FromSeconds(2));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("valkey"),
                    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse,
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).ShouldContain("Healthcheck timed out");
    }

    [Fact]
    public async Task not_every_IConnectionMultiplexer_is_ConnectionMultiplexer()
    {
        using var host = TestHostHelper.Build(webHostBuilder => webHostBuilder
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConnectionMultiplexer>(new NotConnectionMultiplexer());
                services.AddHealthChecks().AddValkey(sp => sp.GetRequiredService<IConnectionMultiplexer>());
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
            }));

        var server = host.GetTestServer();

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private sealed class NotConnectionMultiplexer : IConnectionMultiplexer
    {
        public void AddLibraryNameSuffix(string suffix) => throw new NotImplementedException();

        public EndPoint[] GetEndPoints(bool configuredOnly = false) => [];

#pragma warning disable CS0067
        public override string ToString() => "stop complaining about Nullability";

        public string ClientName => throw new NotImplementedException();

        public string Configuration => throw new NotImplementedException();

        public int TimeoutMilliseconds => throw new NotImplementedException();

        public long OperationCount => throw new NotImplementedException();

        public bool PreserveAsyncOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsConnected => throw new NotImplementedException();

        public bool IsConnecting => throw new NotImplementedException();

        public bool IncludeDetailInExceptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int StormLogThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<RedisErrorEventArgs>? ErrorMessage;

        public event EventHandler<ConnectionFailedEventArgs>? ConnectionFailed;

        public event EventHandler<InternalErrorEventArgs>? InternalError;

        public event EventHandler<ConnectionFailedEventArgs>? ConnectionRestored;

        public event EventHandler<EndPointEventArgs>? ConfigurationChanged;

        public event EventHandler<EndPointEventArgs>? ConfigurationChangedBroadcast;

        public event EventHandler<ServerMaintenanceEvent>? ServerMaintenanceEvent;

        public event EventHandler<HashSlotMovedEventArgs>? HashSlotMoved;

        public void Close(bool allowCommandsToComplete = true) => throw new NotImplementedException();

        public Task CloseAsync(bool allowCommandsToComplete = true) => throw new NotImplementedException();

        public bool Configure(TextWriter? log = null) => throw new NotImplementedException();

        public Task<bool> ConfigureAsync(TextWriter? log = null) => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public ValueTask DisposeAsync() => throw new NotImplementedException();

        public void ExportConfiguration(Stream destination, ExportOptions options = (ExportOptions)(-1)) => throw new NotImplementedException();

        public ServerCounters GetCounters() => throw new NotImplementedException();

        public IDatabase GetDatabase(int db = -1, object? asyncState = null) => throw new NotImplementedException();

        public int GetHashSlot(RedisKey key) => throw new NotImplementedException();

        public IServer GetServer(RedisKey key, object? asyncState = null, CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public IServer GetServer(string host, int port, object? asyncState = null) => throw new NotImplementedException();

        public IServer GetServer(string hostAndPort, object? asyncState = null) => throw new NotImplementedException();

        public IServer GetServer(IPAddress host, int port) => throw new NotImplementedException();

        public IServer GetServer(EndPoint endpoint, object? asyncState = null) => throw new NotImplementedException();

        public IServer[] GetServers() => throw new NotImplementedException();

        public string GetStatus() => throw new NotImplementedException();

        public void GetStatus(TextWriter log) => throw new NotImplementedException();

        public string? GetStormLog() => throw new NotImplementedException();

        public ISubscriber GetSubscriber(object? asyncState = null) => throw new NotImplementedException();

        public int HashSlot(RedisKey key) => throw new NotImplementedException();

        public long PublishReconfigure(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None) => throw new NotImplementedException();

        public void RegisterProfiler(Func<ProfilingSession?> profilingSessionProvider) => throw new NotImplementedException();

        public void ResetStormLog() => throw new NotImplementedException();

        public void Wait(Task task) => throw new NotImplementedException();

        public T Wait<T>(Task<T> task) => throw new NotImplementedException();

        public void WaitAll(params Task[] tasks) => throw new NotImplementedException();
#pragma warning restore CS0067
    }
}

