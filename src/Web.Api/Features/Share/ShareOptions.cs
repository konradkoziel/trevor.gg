namespace Web.Api.Features.Share;

// Where a link preview sends people on. App setting: SITE_URL
public class ShareOptions
{
    // e.g. https://<name>.azurestaticapps.net
    public string SiteUrl { get; set; } = string.Empty;
}
