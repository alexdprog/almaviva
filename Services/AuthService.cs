using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AlmavivaSlotChecker.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace AlmavivaSlotChecker.Services;

public sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<OAuthOptions> options,
    ILogger<AuthService> logger)
{
    private readonly OAuthOptions _options = options.Value;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private AuthTokens? _tokens;
    private string? _codeVerifier;
    private string? _state;

    public bool IsAuthenticated => _tokens is not null && !string.IsNullOrWhiteSpace(_tokens.AccessToken);

    public string BuildAuthorizationUrl(string redirectUri)
    {
        _codeVerifier = CreateCodeVerifier();
        _state = Guid.NewGuid().ToString("N");
        var challenge = CreateCodeChallenge(_codeVerifier);

        var authUri = QueryHelpers.AddQueryString(
            $"{_options.Authority.TrimEnd('/')}/protocol/openid-connect/auth",
            new Dictionary<string, string?>
            {
                ["response_type"] = "code",
                ["client_id"] = _options.ClientId,
                ["scope"] = _options.Scope,
                ["redirect_uri"] = redirectUri,
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256",
                ["state"] = _state
            });

        return authUri;
    }

    public async Task HandleCallbackAsync(string code, string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        if (_codeVerifier is null || _state is null || !string.Equals(_state, state, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("OAuth state validation failed.");
        }

        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["code_verifier"] = _codeVerifier
        };

        using var response = await httpClientFactory.CreateClient("oauth")
            .PostAsync("protocol/openid-connect/token", new FormUrlEncodedContent(payload), cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        _tokens = ParseTokenResponse(body);
        _codeVerifier = null;
        _state = null;
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_tokens is null)
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        if (_tokens.ExpiresAt - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(30))
        {
            return _tokens.AccessToken;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_tokens.ExpiresAt - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(30))
            {
                return _tokens.AccessToken;
            }

            var payload = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _options.ClientId,
                ["refresh_token"] = _tokens.RefreshToken
            };

            using var response = await httpClientFactory.CreateClient("oauth")
                .PostAsync("protocol/openid-connect/token", new FormUrlEncodedContent(payload), cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (body.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Refresh token is invalid (invalid_grant). User has been signed out.");
                    SignOut();
                }

                throw new InvalidOperationException($"Token refresh failed: {body}");
            }

            _tokens = ParseTokenResponse(body);
            return _tokens.AccessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void SignOut() => _tokens = null;

    private static AuthTokens ParseTokenResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
        var refreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty;
        var expiresIn = root.GetProperty("expires_in").GetInt32();

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
        };
    }

    private static string CreateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static string CreateCodeChallenge(string verifier)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(verifier));
        return WebEncoders.Base64UrlEncode(bytes);
    }
}
