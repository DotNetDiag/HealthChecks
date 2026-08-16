var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

app.UseRouting();
app.MapHealthChecksUI();

app.Run();
