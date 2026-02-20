using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AlmavivaSlotChecker.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace AlmavivaSlotChecker.Services;

public sealed class OAuthService(HttpClient httpClient, IOptions<OAuthOptions> options)
{
    private readonly OAuthOptions _options = options.Value;

    public string BuildAuthorizeUrl(string redirectUri, string state)
    {
        var query = new Dictionary<string, string?>
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = _options.Scope,
            ["state"] = state
        };

        return QueryHelpers.AddQueryString(_options.AuthorizeEndpoint, query);
    }

    public async Task<(bool Success, string? AccessToken, string? Email, string? Error)> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        var tokenPayload = new Dictionary<string, string?>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        using var tokenResponse = await httpClient.PostAsync(_options.TokenEndpoint, new FormUrlEncodedContent(tokenPayload), cancellationToken);
        var tokenBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return (false, null, null, $"Token endpoint error: {tokenBody}");
        }

        using var tokenJson = JsonDocument.Parse(tokenBody);
        if (!tokenJson.RootElement.TryGetProperty("access_token", out var accessTokenEl))
        {
            return (false, null, null, "В ответе token endpoint отсутствует access_token.");
        }

        var accessToken = accessTokenEl.GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return (false, null, null, "Пустой access_token.");
        }

        var email = await TryResolveEmailAsync(accessToken, cancellationToken);
        return (true, accessToken, email, null);
    }

    private async Task<string?> TryResolveEmailAsync(string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.UserInfoEndpoint))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var userInfo = JsonDocument.Parse(body);

        if (userInfo.RootElement.TryGetProperty("email", out var emailElement))
        {
            return emailElement.GetString();
        }

        return null;
    }
}
