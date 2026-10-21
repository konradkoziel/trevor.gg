using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Web.Api.Features.Auth;
using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

public class DiscordAuthFunctionTests
{
    // Cookie of a login that went through the consent screen / of the silent first attempt
    private const string AfterConsent = "s1.c";
    private const string AfterSilent = "s1.s";

    private static DefaultHttpContext Callback(string? code, string? state = "s1", string? cookie = AfterConsent, string? error = null)
    {
        var context = new DefaultHttpContext();
        var query = new Dictionary<string, string?>();
        if (code is not null) query["code"] = code;
        if (state is not null) query["state"] = state;
        if (error is not null) query["error"] = error;
        context.Request.QueryString = QueryString.Create(query);
        if (cookie is not null)
            context.Request.Headers.Cookie = $"{LoginState.CookieName}={cookie}";
        return context;
    }

    private static Task<IActionResult> RunCallback(DefaultHttpContext context, FakeDiscordOAuth discord, IOptions<DiscordOAuthOptions>? options = null) =>
        new DiscordCallbackFunction(discord, TestAuth.Tokens(), options ?? TestAuth.Configured(), NullLogger<DiscordCallbackFunction>.Instance)
            .Run(context.Request, CancellationToken.None);

    private static string StateIn(string url) => QueryHelpers.ParseQuery(new Uri(url).Query)["state"].ToString();

    private static async Task<string> NameAndAvatarFrom(IActionResult result)
    {
        var url = Assert.IsType<RedirectResult>(result).Url;
        Assert.StartsWith($"{TestAuth.SiteUrl}/#{SiteRedirects.SessionFragment}=", url);
        var user = await TestAuth.Tokens().ReadAsync(TestAuth.RequestWithBearer(Uri.UnescapeDataString(url[(url.IndexOf('=') + 1)..])));
        return $"{user!.Name} {user.AvatarUrl}";
    }

    [Fact]
    public void Login_TriesSilentlyFirst_WithTheStateFromItsCookie()
    {
        var context = new DefaultHttpContext();

        var result = new DiscordLoginFunction(new FakeDiscordOAuth(), TestAuth.Configured()).Run(context.Request);

        var url = Assert.IsType<RedirectResult>(result).Url;
        Assert.Contains("silent=true", url);
        var cookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains($"{LoginState.CookieName}={StateIn(url)}.s", cookie);
        Assert.Contains("httponly", cookie);
        Assert.Contains("secure", cookie);
        Assert.Contains("samesite=lax", cookie);
    }

    [Fact]
    public void Login_NotConfigured_Is503()
    {
        var result = new DiscordLoginFunction(new FakeDiscordOAuth(), TestAuth.NotConfigured()).Run(new DefaultHttpContext().Request);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Callback_LogsIn_AndSendsTheTokenInTheFragment()
    {
        var discord = new FakeDiscordOAuth(TestAuth.Ann);

        var result = await RunCallback(Callback("the-code"), discord);

        Assert.Equal($"Ann {DiscordAvatars.Url(TestAuth.Ann.Id, TestAuth.Ann.Avatar)}", await NameAndAvatarFrom(result));
        Assert.Equal("the-code", discord.ExchangedCode);
    }

    [Fact]
    public async Task SilentAttemptRefused_AsksForConsentOnce()
    {
        var context = Callback(code: null, cookie: AfterSilent, error: "consent_required");

        var result = await RunCallback(context, new FakeDiscordOAuth(TestAuth.Ann));

        var url = Assert.IsType<RedirectResult>(result).Url;
        Assert.Contains("silent=false", url);
        Assert.Contains($"{LoginState.CookieName}={StateIn(url)}.c", context.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task CancelOnTheConsentScreen_Fails()
    {
        var result = await RunCallback(Callback(code: null, error: "access_denied"), new FakeDiscordOAuth(TestAuth.Ann));

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
    }

    [Theory]
    [InlineData("s1", "other.c")] // forged or stale login
    [InlineData("s1", null)] // cookie expired or blocked
    [InlineData("s1", "s1")] // cookie in the old format
    [InlineData(null, AfterConsent)]
    public async Task Callback_WithWrongState_FailsWithoutCallingDiscord(string? state, string? cookie)
    {
        var discord = new FakeDiscordOAuth(TestAuth.Ann);

        var result = await RunCallback(Callback("the-code", state, cookie), discord);

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
        Assert.Null(discord.ExchangedCode);
    }

    [Fact]
    public async Task Callback_ClearsTheStateCookie()
    {
        var context = Callback("the-code");

        await RunCallback(context, new FakeDiscordOAuth(TestAuth.Ann));

        Assert.Contains($"{LoginState.CookieName}=;", context.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Callback_DiscordRejectsTheCode_Fails()
    {
        var result = await RunCallback(Callback("old-code"), new FakeDiscordOAuth(user: null));

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public async Task GuildOnly_SomeoneFromOutside_IsTurnedAway()
    {
        var result = await RunCallback(Callback("the-code"), new FakeDiscordOAuth(TestAuth.Ann, member: null), TestAuth.GuildOnly());

        Assert.Equal($"{TestAuth.SiteUrl}/#login-error=not-member", Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public async Task GuildOnly_Member_GetsTheirServerNickAndAvatar()
    {
        var member = new DiscordGuildMember("Annie", "g1");

        var result = await RunCallback(Callback("the-code"), new FakeDiscordOAuth(TestAuth.Ann, member), TestAuth.GuildOnly());

        Assert.Equal($"Annie {DiscordAvatars.GuildUrl(TestAuth.GuildId, TestAuth.Ann.Id, "g1")}", await NameAndAvatarFrom(result));
    }

    [Fact]
    public async Task GuildOnly_MemberWithoutServerProfile_KeepsTheirOwn()
    {
        var result = await RunCallback(Callback("the-code"), new FakeDiscordOAuth(TestAuth.Ann, new DiscordGuildMember(null, null)), TestAuth.GuildOnly());

        Assert.Equal($"Ann {DiscordAvatars.Url(TestAuth.Ann.Id, TestAuth.Ann.Avatar)}", await NameAndAvatarFrom(result));
    }

    [Fact]
    public async Task Callback_NotConfigured_Is503()
    {
        var result = await RunCallback(Callback("c"), new FakeDiscordOAuth(TestAuth.Ann), TestAuth.NotConfigured());

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }
}
