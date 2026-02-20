using System.Net;
using AlmavivaSlotChecker.Components;
using AlmavivaSlotChecker.Data;
using AlmavivaSlotChecker.Models;
using AlmavivaSlotChecker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddSingleton<OAuthStateStore>();

builder.Services.AddHttpClient<AlmavivaClient>(client =>
    {
        client.BaseAddress = new Uri("https://visa.almaviva-russia.ru/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        CookieContainer = new CookieContainer(),
        AutomaticDecompression = DecompressionMethods.All,
        UseCookies = true
    });

builder.Services.AddHttpClient<OAuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/auth/login", (HttpContext httpContext, OAuthService oAuthService, OAuthStateStore stateStore, IConfiguration configuration) =>
{
    var callbackPath = configuration[$"{OAuthOptions.SectionName}:CallbackPath"] ?? "/auth/callback";
    var redirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{callbackPath}";

    var state = stateStore.Create();
    httpContext.Response.Cookies.Append("oauth_state", state, new CookieOptions
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = httpContext.Request.IsHttps,
        Expires = DateTimeOffset.UtcNow.AddMinutes(10)
    });

    var authorizeUrl = oAuthService.BuildAuthorizeUrl(redirectUri, state);
    return Results.Redirect(authorizeUrl);
});

app.MapGet("/auth/callback", async (HttpContext httpContext, OAuthService oAuthService, OAuthStateStore stateStore, AppointmentService appointmentService) =>
{
    var query = httpContext.Request.Query;
    var code = query["code"].ToString();
    var state = query["state"].ToString();
    var oauthError = query["error"].ToString();

    if (!string.IsNullOrWhiteSpace(oauthError))
    {
        await appointmentService.AuditLoginAsync(new LoginRequest { IsSuccess = false, ErrorMessage = oauthError });
        return Results.Redirect($"/login?error={Uri.EscapeDataString(oauthError)}");
    }

    if (!httpContext.Request.Cookies.TryGetValue("oauth_state", out var storedState) || storedState != state || !stateStore.ValidateAndConsume(state))
    {
        await appointmentService.AuditLoginAsync(new LoginRequest { IsSuccess = false, ErrorMessage = "Invalid state" });
        return Results.Redirect("/login?error=Invalid%20state");
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        await appointmentService.AuditLoginAsync(new LoginRequest { IsSuccess = false, ErrorMessage = "Missing code" });
        return Results.Redirect("/login?error=Missing%20authorization%20code");
    }

    var callbackPath = httpContext.Request.Path;
    var redirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{callbackPath}";

    var result = await oAuthService.ExchangeCodeAsync(code, redirectUri);

    if (!result.Success)
    {
        await appointmentService.AuditLoginAsync(new LoginRequest { IsSuccess = false, ErrorMessage = result.Error });
        return Results.Redirect($"/login?error={Uri.EscapeDataString(result.Error ?? "OAuth error")}");
    }

    httpContext.Response.Cookies.Append("almaviva_access_token", result.AccessToken!, new CookieOptions
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = httpContext.Request.IsHttps,
        Expires = DateTimeOffset.UtcNow.AddHours(1)
    });

    var email = result.Email ?? "oauth-user";
    await appointmentService.AuditLoginAsync(new LoginRequest { Email = email, IsSuccess = true });
    return Results.Redirect($"/login?success=1&email={Uri.EscapeDataString(email)}");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
