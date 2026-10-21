namespace Web.Api.Features.Bot;

// The bot's access to this API. App settings: BOT_API_KEY (shared with the bot's WEB_API_KEY), SITE_URL
public class BotApiOptions
{
    public string Key { get; set; } = string.Empty;

    // Base of the links the bot posts, e.g. https://<name>.azurestaticapps.net
    public string SiteUrl { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Key);
}
