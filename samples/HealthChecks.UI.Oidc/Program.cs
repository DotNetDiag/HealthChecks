using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

const string HEALTH_CHECKS_UI_POLICY = nameof(HEALTH_CHECKS_UI_POLICY);

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage()
    .Services
    .AddAuthorization(cfg =>
    {
        cfg.AddPolicy(name: HEALTH_CHECKS_UI_POLICY, cfgPolicy =>
        {
            cfgPolicy.AddRequirements().RequireAuthenticatedUser();
            cfgPolicy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
        });
    })
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options => options.SlidingExpiration = true)
    .AddOpenIdConnect(options =>
    {
        options.Authority = "https://demo.identityserver.io";
        options.ClientId = "interactive.confidential";
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        options.SaveTokens = true;
    })
    .Services
    .AddHealthChecks()
    .AddUrlGroup(new Uri("https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks"))
    .Services
    .AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI().RequireAuthorization(HEALTH_CHECKS_UI_POLICY);
app.MapDefaultControllerRoute();

app.Run();
