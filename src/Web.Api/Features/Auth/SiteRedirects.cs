namespace Web.Api.Features.Auth;

// Where the callback sends the browser. The token goes in the #fragment, which browsers never send to any server;
// the site reads it, stores it and removes it from the address bar (src/web/src/auth/session.ts)
public static class SiteRedirects
{
    public const string SessionFragment = "session";
    public const string LoginErrorFragment = "login-error";

    public static string LoggedIn(string siteUrl, string token) => $"{siteUrl.TrimEnd('/')}/#{SessionFragment}={Uri.EscapeDataString(token)}";

    public static string LoginFailed(string siteUrl) => $"{siteUrl.TrimEnd('/')}/#{LoginErrorFragment}";
}
