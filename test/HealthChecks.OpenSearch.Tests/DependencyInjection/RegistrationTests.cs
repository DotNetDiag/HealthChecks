using OpenSearch.Client;

namespace HealthChecks.OpenSearch.Tests.DependencyInjection;

public class opensearch_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenSearch("http://localhost:9200");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("opensearch");
        check.ShouldBeOfType<OpenSearchHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenSearch("http://localhost:9200", name: "my-opensearch");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-opensearch");
        check.ShouldBeOfType<OpenSearchHealthCheck>();
    }

    [Fact]
    public void create_client_with_user_configured_request_timeout()
    {
        var services = new ServiceCollection();
        var settings = new OpenSearchOptions();

        services.AddHealthChecks().AddOpenSearch(setup =>
        {
            setup.UseServer("http://localhost:9200");
            setup.RequestTimeout = TimeSpan.FromSeconds(6);
            settings = setup;
        });

        settings.RequestTimeout.ShouldNotBeNull();
        settings.RequestTimeout.ShouldBe(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public void create_client_with_configured_healthcheck_timeout_when_no_request_timeout_is_configured()
    {
        var services = new ServiceCollection();
        var settings = new OpenSearchOptions();

        services.AddHealthChecks().AddOpenSearch(setup =>
        {
            setup.UseServer("http://localhost:9200");
            settings = setup;
        }, timeout: TimeSpan.FromSeconds(7));

        settings.RequestTimeout.ShouldNotBeNull();
        settings.RequestTimeout.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void create_client_with_no_timeout_when_no_option_is_configured()
    {
        var services = new ServiceCollection();
        var settings = new OpenSearchOptions();

        services.AddHealthChecks().AddOpenSearch(setup =>
        {
            setup.UseServer("http://localhost:9200");
            settings = setup;
        });

        settings.RequestTimeout.ShouldBeNull();
    }

    [Fact]
    public void throw_exception_when_create_client_without_server()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddHealthChecks().AddOpenSearch(_ => { }));
    }

    [Fact]
    public void configure_basic_authentication()
    {
        var settings = new OpenSearchOptions();

        settings.UseBasicAuthentication("user", "password");

        settings.AuthenticateWithBasicCredentials.ShouldBeTrue();
        settings.UserName.ShouldBe("user");
        settings.Password.ShouldBe("password");
    }

    [Fact]
    public void configure_api_key_authentication()
    {
        var settings = new OpenSearchOptions();

        settings.UseApiKey("id", "api-key");

        settings.AuthenticateWithApiKey.ShouldBeTrue();
        settings.ApiKeyId.ShouldBe("id");
        settings.ApiKey.ShouldBe("api-key");
    }

    [Fact]
    public void client_should_resolve_from_interface_in_di()
    {
        IOpenSearchClient client = new OpenSearchClient(new Uri("http://localhost:9200"));
        var services = new ServiceCollection();
        services.AddSingleton(client);

        services.AddHealthChecks()
            .AddOpenSearch();

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("opensearch");
        check.ShouldBeOfType<OpenSearchHealthCheck>();
    }

    [Fact]
    public void client_should_resolve_from_concrete_client_in_di()
    {
        var client = new OpenSearchClient(new Uri("http://localhost:9200"));
        var services = new ServiceCollection();
        services.AddSingleton(client);

        services.AddHealthChecks()
            .AddOpenSearch();

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("opensearch");
        check.ShouldBeOfType<OpenSearchHealthCheck>();
    }

    [Fact]
    public void use_client_factory()
    {
        IOpenSearchClient client = new OpenSearchClient(new Uri("http://localhost:9200"));
        var services = new ServiceCollection();
        var factoryCalled = false;

        services.AddHealthChecks().AddOpenSearch(clientFactory: sp =>
        {
            factoryCalled = true;
            return client;
        });

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<OpenSearchHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }
}
