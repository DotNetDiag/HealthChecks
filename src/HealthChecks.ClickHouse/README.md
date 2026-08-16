## ClickHouse Health Check

This health check verifies the ability to communicate with [ClickHouse](https://www.clickhouse.com/). It uses the [ClickHouse.Driver](https://www.nuget.org/packages/ClickHouse.Driver) library.

## Usage

```csharp
void Configure(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddClickHouse("Host=ch;Username=default;Password=test;Database=default");
}
```

## Advanced usage

If you want to manage the HTTP connection manually (e.g. via `IHttpClientFactory`), you can use the factory overload:

```csharp
void Configure(IServiceCollection services)
{
    services.AddHttpClient("ClickHouseClient");
    services.AddHealthChecks().AddClickHouse(static sp => {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        return new ClickHouseConnection("Host=ch;Username=default;Password=test;Database=default", httpClientFactory, "ClickHouseClient");
    });
}
```
