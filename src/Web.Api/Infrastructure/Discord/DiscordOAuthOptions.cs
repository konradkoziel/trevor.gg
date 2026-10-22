namespace Web.Api.Infrastructure.Discord;

// "Login with Discord". App settings: DISCORD_APP_ID, DISCORD_CLIENT_SECRET, DISCORD_REDIRECT_URI, SITE_URL, DISCORD_GUILD_ID
public class DiscordOAuthOptions
{
    // The bot's Discord application id
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    // Must be listed under OAuth2 → Redirects in the Developer Portal, character for character
    public string RedirectUri { get; set; } = string.Empty;

    // Where the callback sends the browser back to, e.g. https://<name>.azurestaticapps.net
    public string SiteUrl { get; set; } = string.Empty;

    // Trevor's server: when set, only its members can log in. Empty lets any Discord user in
    public string? GuildId { get; set; }

    public bool IsGuildOnly => !string.IsNullOrWhiteSpace(GuildId);

    // Login stays off (503) until every value is set, the rest of the API works without it
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret)
        && Uri.IsWellFormedUriString(RedirectUri, UriKind.Absolute) && Uri.IsWellFormedUriString(SiteUrl, UriKind.Absolute);
}
