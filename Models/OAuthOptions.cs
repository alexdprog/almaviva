namespace AlmavivaSlotChecker.Models;

public sealed class OAuthOptions
{
    public const string SectionName = "OAuth";

    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = "aa-visasys-public";
    public string Scope { get; set; } = "openid profile offline_access";
}
