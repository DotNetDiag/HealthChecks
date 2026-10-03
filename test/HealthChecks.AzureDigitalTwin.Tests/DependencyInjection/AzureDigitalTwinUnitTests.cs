namespace HealthChecks.AzureDigitalTwin.Tests;

public class azure_digital_twin_registration_should
{
    private const string RESOURCE_ID = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin";

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", RESOURCE_ID);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuredigitaltwin");
        check.ShouldBeOfType<AzureDigitalTwinSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", RESOURCE_ID, name: "azuredigitaltwincheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuredigitaltwincheck");
        check.ShouldBeOfType<AzureDigitalTwinSubscriptionHealthCheck>();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin(string.Empty, string.Empty, string.Empty, RESOURCE_ID);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        Should.Throw<ArgumentNullException>(() => registration.Factory(serviceProvider));
    }

    [Fact]
    public void add_health_check_when_properly_configured_by_credentials()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin(credentials: new MockTokenCredentials(), resourceId: RESOURCE_ID);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuredigitaltwin");
        check.ShouldBeOfType<AzureDigitalTwinSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured_by_credentials()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin(new MockTokenCredentials(), RESOURCE_ID, name: "azuredigitaltwincheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuredigitaltwincheck");
        check.ShouldBeOfType<AzureDigitalTwinSubscriptionHealthCheck>();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided_by_credentials()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureDigitalTwin(null!, RESOURCE_ID);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        Should.Throw<ArgumentNullException>(() => registration.Factory(serviceProvider));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void preserve_registration_metadata(bool useTokenCredential)
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        string[] tags = ["azure", "ready"];
        var timeout = TimeSpan.FromSeconds(30);

        if (useTokenCredential)
        {
            builder.AddAzureDigitalTwin(new MockTokenCredentials(), RESOURCE_ID,
                name: "target-twin", failureStatus: HealthStatus.Degraded, tags: tags, timeout: timeout);
        }
        else
        {
            builder.AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", RESOURCE_ID,
                name: "target-twin", failureStatus: HealthStatus.Degraded, tags: tags, timeout: timeout);
        }

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registration = options.Value.Registrations.Single();

        registration.Name.ShouldBe("target-twin");
        registration.FailureStatus.ShouldBe(HealthStatus.Degraded);
        registration.Tags.ShouldBe(tags, ignoreOrder: true);
        registration.Timeout.ShouldBe(timeout);
        registration.Factory(serviceProvider).ShouldBeOfType<AzureDigitalTwinSubscriptionHealthCheck>();
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(false, null)]
    [InlineData(false, "")]
    public void fail_when_resource_id_is_missing(bool useTokenCredential, string? resourceId)
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();

        if (useTokenCredential)
        {
            builder.AddAzureDigitalTwin(new MockTokenCredentials(), resourceId!);
        }
        else
        {
            builder.AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", resourceId!);
        }

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registration = options.Value.Registrations.Single();

        Should.Throw<ArgumentNullException>(() => registration.Factory(serviceProvider));
    }

    [Theory]
    [InlineData(true, "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test/providers/Microsoft.KeyVault/vaults/test-vault")]
    [InlineData(false, "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test/providers/Microsoft.KeyVault/vaults/test-vault")]
    [InlineData(true, "/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin")]
    [InlineData(false, "/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin")]
    [InlineData(true, "/subscriptions/00000000-0000-0000-0000-000000000000/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin")]
    [InlineData(false, "/subscriptions/00000000-0000-0000-0000-000000000000/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin")]
    public void fail_when_resource_id_does_not_identify_a_full_digital_twins_resource(bool useTokenCredential, string resourceId)
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();

        if (useTokenCredential)
        {
            builder.AddAzureDigitalTwin(new MockTokenCredentials(), resourceId);
        }
        else
        {
            builder.AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", resourceId);
        }

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registration = options.Value.Registrations.Single();

        Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider)).ParamName.ShouldBe("resourceId");
    }
}
