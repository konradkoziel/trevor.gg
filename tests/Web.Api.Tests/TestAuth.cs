using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Infrastructure.Discord;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public static class TestAuth
{
    public const string SiteUrl = "https://site.test";

    public static readonly DiscordOAuthUser Ann = new("80351110224678912", "ann_123", "Ann", "a1b2c3");

    public static IOptions<DiscordOAuthOptions> Configured() => Options.Create(new DiscordOAuthOptions
    {
        ClientId = "app-id",
        ClientSecret = "secret",
        RedirectUri = "https://api.test/api/auth/discord/callback",
        SiteUrl = SiteUrl,
    });

    public static IOptions<DiscordOAuthOptions> NotConfigured() => Options.Create(new DiscordOAuthOptions());

    public static SessionTokens Tokens(FakeTimeProvider? time = null, byte keyFill = 7) =>
        new(new FakeSigningKeyStore(keyFill), time ?? new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)));

    public static HttpRequest RequestWithBearer(string? token)
    {
        var context = new DefaultHttpContext();
        if (token is not null)
            context.Request.Headers.Authorization = $"Bearer {token}";
        return context.Request;
    }
}
