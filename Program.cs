using AlmavivaSlotChecker.Components;
using AlmavivaSlotChecker.Models;
using AlmavivaSlotChecker.Services;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.SectionName));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SlotCheckService>();
builder.Services.AddSingleton<CheckerStateService>();
builder.Services.AddSingleton<AutoCheckCoordinator>();
builder.Services.AddHostedService<AutomaticCheckBackgroundService>();
builder.Services.AddSingleton<TelegramNotificationService>();

builder.Services.AddHttpClient("oauth", client =>
{
    var authority = builder.Configuration[$"{OAuthOptions.SectionName}:Authority"];
    client.BaseAddress = new Uri($"{authority?.TrimEnd('/')}/");
});

builder.Services.AddHttpClient("visa-api", client =>
{
    client.BaseAddress = new Uri("https://visaapi.almaviva-russia.ru/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("telegram", client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/auth/login", (AuthService authService, HttpContext context) =>
{
    var callbackUrl = $"{context.Request.Scheme}://{context.Request.Host}/auth/callback";
    var url = authService.BuildAuthorizationUrl(callbackUrl);
    return Results.Redirect(url);
});

app.MapGet("/auth/callback", async (string? code, string? state, AuthService authService, HttpContext context, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
    {
        return Results.Redirect("/?error=missing_oauth_code");
    }

    var callbackUrl = $"{context.Request.Scheme}://{context.Request.Host}/auth/callback";

    try
    {
        await authService.HandleCallbackAsync(code, state, callbackUrl, ct);
        return Results.Redirect("/");
    }
    catch
    {
        return Results.Redirect("/?error=oauth_callback_failed");
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
