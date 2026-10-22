namespace trevor.Infrastructure.Web;

// Pages of the website the bot links to; the paths match the routes in src/web/src/App.tsx
public static class SiteLinks
{
    // Both teams of one game; null while WEB_SITE_URL isn't set
    public static string? Match(string? siteUrl, string matchId) =>
        string.IsNullOrWhiteSpace(siteUrl) ? null : $"{siteUrl.TrimEnd('/')}/mecz/{Uri.EscapeDataString(matchId)}";
}
