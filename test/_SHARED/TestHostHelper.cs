using Microsoft.Extensions.Hosting;

public static class TestHostHelper
{
    public static IHost Build(Action<IWebHostBuilder> configureWebHost) => Build(startHost: true, configureWebHost);

    public static IHost Build(bool startHost, Action<IWebHostBuilder> configureWebHost)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseTestServer();
                configureWebHost(webHostBuilder);
            })
            .Build();

        if (startHost)
        {
            host.StartAsync().GetAwaiter().GetResult();
        }

        return host;
    }
}
