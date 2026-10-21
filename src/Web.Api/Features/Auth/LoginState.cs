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

    public static string Start(HttpResponse response)
    {
        var state = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(StateBytes));
        response.Cookies.Append(CookieName, state, CookieOptions(Lifetime));
        return state;
    }

    // Single use: the cookie is cleared whatever the outcome
    public static bool Finish(HttpRequest request, HttpResponse response, string? returnedState)
    {
        var expected = request.Cookies[CookieName];
        response.Cookies.Delete(CookieName, CookieOptions(TimeSpan.Zero));

        return !string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(returnedState)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(returnedState));
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
