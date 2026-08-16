using NSubstitute;
using StackExchange.Redis;

namespace HealthChecks.Redis.Tests.DependencyInjection;

public class redis_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddRedis("connectionstring");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("redis");
        check.ShouldBeOfType<RedisHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddRedis("connectionstring", name: "my-redis");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-redis");
        check.ShouldBeOfType<RedisHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddRedis(_ =>
            {
                factoryCalled = true;
                return "connectionstring";
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("redis");
        check.ShouldBeOfType<RedisHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_with_connection_multiplexer_when_properly_configured()
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();

        var services = new ServiceCollection();

        services.AddHealthChecks()
            .AddRedis(connectionMultiplexer, name: "my-redis");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-redis");
        check.ShouldBeOfType<RedisHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_multiplexer_when_properly_configured()
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        var services = new ServiceCollection();

        services.AddSingleton(connectionMultiplexer);
        var factoryCalled = false;

        services.AddHealthChecks()
            .AddRedis(sp =>
            {
                factoryCalled = true;
                return sp.GetRequiredService<IConnectionMultiplexer>();
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("redis");
        check.ShouldBeOfType<RedisHealthCheck>();
        // the factory is called when it's used for the first time, as it can throw
        factoryCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task add_health_check_with_async_connection_multiplexer_factory_when_properly_configured()
    {
        var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        connectionMultiplexer.GetEndPoints(configuredOnly: true).Returns([]);

        var services = new ServiceCollection();
        var factoryCalled = false;
        CancellationToken capturedCancellationToken = default;

        services.AddHealthChecks()
            .AddRedis(async (_, cancellationToken) =>
            {
                factoryCalled = true;
                capturedCancellationToken = cancellationToken;
                await Task.Yield();
                return connectionMultiplexer;
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("redis");
        check.ShouldBeOfType<RedisHealthCheck>();
        factoryCalled.ShouldBeFalse();

        using var cancellationTokenSource = new CancellationTokenSource();
        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = registration },
            cancellationTokenSource.Token);

        result.Status.ShouldBe(HealthStatus.Healthy);
        factoryCalled.ShouldBeTrue();
        capturedCancellationToken.ShouldBe(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task return_timeout_when_async_connection_multiplexer_factory_is_cancelled()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddRedis((_, cancellationToken) =>
                Task.FromCanceled<IConnectionMultiplexer>(cancellationToken));

        using var serviceProvider = services.BuildServiceProvider();
        var registration = serviceProvider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        var result = await check.CheckHealthAsync(
            new HealthCheckContext { Registration = registration },
            cancellationTokenSource.Token);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldBe("Healthcheck timed out");
    }
}
