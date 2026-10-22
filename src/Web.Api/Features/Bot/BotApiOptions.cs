namespace Web.Api.Features.Bot;

// The bot's access to this API. App settings: TREVOR_WEB_KEY (the bot has the same one), SITE_URL
public class BotApiOptions
{
    public string Key { get; set; } = string.Empty;

    // Base of the links the bot posts, e.g. https://<name>.azurestaticapps.net
    public string SiteUrl { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Key);
}
