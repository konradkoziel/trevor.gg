namespace trevor.Infrastructure.Web;

// The website's API. App settings WEB_API_URL, WEB_API_KEY, WEB_SITE_URL, set by the website's deploy (web.yml).
// Optional: without them /rank and /roast need a nick and /build says the site isn't connected
public class WebApiOptions
{
    public string? Url { get; set; }
    public string? Key { get; set; }
    public string? SiteUrl { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(Key);
}
