using Microsoft.Extensions.Hosting;

public static class TestHostHelper
{
    public static IHost Build(Action<IWebHostBuilder> configureWebHost)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseTestServer();
                configureWebHost(webHostBuilder);
            })
            .Build();

        host.StartAsync().GetAwaiter().GetResult();

        return host;
    }
}
