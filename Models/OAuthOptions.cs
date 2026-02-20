namespace AlmavivaSlotChecker.Models;

public sealed class OAuthOptions
{
    public const string SectionName = "OAuth";

    public string AuthorizeEndpoint { get; set; } = "https://visa.almaviva-russia.ru/oauth2/authorize";
    public string TokenEndpoint { get; set; } = "https://visa.almaviva-russia.ru/oauth2/token";
    public string UserInfoEndpoint { get; set; } = "https://visa.almaviva-russia.ru/oauth2/userinfo";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Scope { get; set; } = "openid profile email";
    public string CallbackPath { get; set; } = "/auth/callback";
}
