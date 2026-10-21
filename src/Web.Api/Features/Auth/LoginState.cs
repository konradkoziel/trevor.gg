using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Web.Api.Features.Auth;

// OAuth "state": a random value in a cookie that must come back from Discord unchanged,
// so nobody can finish a login in your browser that you didn't start (CSRF)
public static class LoginState
{
    public const string CookieName = "trevor_login_state";

    // Only the login and callback functions need to see it
    private const string CookiePath = "/api/auth/discord";

    // Time to click "Authorize" on Discord
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private const int StateBytes = 32;

    // The cookie holds "<state>.<attempt>"; base64url never contains a dot
    private const char Separator = '.';
    private const string SilentAttempt = "s";
    private const string ConsentAttempt = "c";

    public static string Start(HttpResponse response, bool silent)
    {
        var state = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(StateBytes));
        response.Cookies.Append(CookieName, $"{state}{Separator}{(silent ? SilentAttempt : ConsentAttempt)}", CookieOptions(Lifetime));
        return state;
    }

    // Single use: the cookie is cleared whatever the outcome
    public static LoginCheck Finish(HttpRequest request, HttpResponse response, string? returnedState)
    {
        var cookie = request.Cookies[CookieName];
        response.Cookies.Delete(CookieName, CookieOptions(TimeSpan.Zero));

        var parts = cookie?.Split(Separator);
        if (parts is not [var expected, var attempt] || string.IsNullOrEmpty(returnedState))
            return new LoginCheck(false, false);

        var valid = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(returnedState));
        return new LoginCheck(valid, attempt == SilentAttempt);
    }

    // Lax: the cookie still comes along on the top-level redirect back from discord.com
    private static CookieOptions CookieOptions(TimeSpan maxAge) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = CookiePath,
        MaxAge = maxAge,
    };
}
