using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Web.Api.Features.Auth;
using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

public class DiscordAuthFunctionTests
{
    private static DefaultHttpContext Callback(string? code, string? state, string? cookieState)
    {
        var context = new DefaultHttpContext();
        var query = new Dictionary<string, string?>();
        if (code is not null) query["code"] = code;
        if (state is not null) query["state"] = state;
        context.Request.QueryString = QueryString.Create(query);
        if (cookieState is not null)
            context.Request.Headers.Cookie = $"{LoginState.CookieName}={cookieState}";
        return context;
    }

    private static Task<IActionResult> RunCallback(DefaultHttpContext context, FakeDiscordOAuth discord, Microsoft.Extensions.Options.IOptions<DiscordOAuthOptions>? options = null) =>
        new DiscordCallbackFunction(discord, TestAuth.Tokens(), options ?? TestAuth.Configured(), NullLogger<DiscordCallbackFunction>.Instance)
            .Run(context.Request, CancellationToken.None);

    [Fact]
    public void Login_RedirectsToDiscord_WithTheStateFromItsCookie()
    {
        var context = new DefaultHttpContext();

        var result = new DiscordLoginFunction(new FakeDiscordOAuth(), TestAuth.Configured()).Run(context.Request);

        var url = new Uri(Assert.IsType<RedirectResult>(result).Url);
        var state = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(url.Query)["state"].ToString();
        var cookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains($"{LoginState.CookieName}={state}", cookie);
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

        var result = await RunCallback(Callback("the-code", "s1", "s1"), discord);

        var url = Assert.IsType<RedirectResult>(result).Url;
        Assert.StartsWith($"{TestAuth.SiteUrl}/#{SiteRedirects.SessionFragment}=", url);
        Assert.Equal("the-code", discord.ExchangedCode);

        var token = Uri.UnescapeDataString(url[(url.IndexOf('=') + 1)..]);
        var user = await TestAuth.Tokens().ReadAsync(TestAuth.RequestWithBearer(token));
        Assert.Equal(TestAuth.Ann.Id, user!.DiscordId);
        Assert.Equal("Ann", user.Name);
        Assert.Equal(DiscordAvatars.Url(TestAuth.Ann.Id, TestAuth.Ann.Avatar), user.AvatarUrl);
    }

    [Theory]
    [InlineData("s1", "other")] // forged or stale login
    [InlineData("s1", null)] // cookie expired or blocked
    [InlineData(null, "s1")]
    public async Task Callback_WithWrongState_FailsWithoutCallingDiscord(string? state, string? cookieState)
    {
        var discord = new FakeDiscordOAuth(TestAuth.Ann);

        var result = await RunCallback(Callback("the-code", state, cookieState), discord);

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
        Assert.Null(discord.ExchangedCode);
    }

    [Fact]
    public async Task Callback_ClearsTheStateCookie()
    {
        var context = Callback("the-code", "s1", "s1");

        await RunCallback(context, new FakeDiscordOAuth(TestAuth.Ann));

        Assert.Contains($"{LoginState.CookieName}=;", context.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Callback_UserCancelledOnDiscord_Fails()
    {
        var result = await RunCallback(Callback(code: null, "s1", "s1"), new FakeDiscordOAuth(TestAuth.Ann));

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public async Task Callback_DiscordRejectsTheCode_Fails()
    {
        var result = await RunCallback(Callback("old-code", "s1", "s1"), new FakeDiscordOAuth(user: null));

        Assert.Equal(SiteRedirects.LoginFailed(TestAuth.SiteUrl), Assert.IsType<RedirectResult>(result).Url);
    }

    [Fact]
    public async Task Callback_NotConfigured_Is503()
    {
        var result = await RunCallback(Callback("c", "s1", "s1"), new FakeDiscordOAuth(TestAuth.Ann), TestAuth.NotConfigured());

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }
}
