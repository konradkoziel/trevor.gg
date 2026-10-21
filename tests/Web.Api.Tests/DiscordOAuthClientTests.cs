using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

public class DiscordOAuthClientTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(HttpRequestMessage Request, string? Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add((request, request.Content is null ? null : await request.Content.ReadAsStringAsync(ct)));
            return respond(request);
        }
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static DiscordOAuthClient Client(StubHandler handler, bool guildOnly = false) =>
        new(new HttpClient(handler), guildOnly ? TestAuth.GuildOnly() : TestAuth.Configured(), NullLogger<DiscordOAuthClient>.Instance);

    private const string TokenJson = """{"access_token":"at-1","token_type":"Bearer","expires_in":604800,"scope":"identify"}""";
    private const string UserJson = """{"id":"80351110224678912","username":"ann_123","global_name":"Ann","avatar":"a1b2c3"}""";

    [Fact]
    public void AuthorizeUrl_AsksOnlyForIdentify()
    {
        var url = Client(new StubHandler(_ => Json("{}"))).AuthorizeUrl("st", silent: false);

        var query = QueryHelpers.ParseQuery(url.Query);
        Assert.StartsWith(DiscordOAuthClient.AuthorizeEndpoint, url.AbsoluteUri);
        Assert.Equal("app-id", query["client_id"]);
        Assert.Equal("code", query["response_type"]);
        Assert.Equal("https://api.test/api/auth/discord/callback", query["redirect_uri"]);
        Assert.Equal("identify", query["scope"]);
        Assert.Equal("st", query["state"]);
        Assert.False(query.ContainsKey("prompt"));
    }

    [Fact]
    public void SilentAttempt_SkipsTheConsentScreen()
    {
        var url = Client(new StubHandler(_ => Json("{}"))).AuthorizeUrl("st", silent: true);

        Assert.Equal("none", QueryHelpers.ParseQuery(url.Query)["prompt"]);
    }

    [Fact]
    public void GuildOnly_AsksForMembershipOfThatServer()
    {
        var url = Client(new StubHandler(_ => Json("{}")), guildOnly: true).AuthorizeUrl("st", silent: false);

        Assert.Equal("identify guilds.members.read", QueryHelpers.ParseQuery(url.Query)["scope"]);
    }

    [Fact]
    public async Task GuildOnly_ReadsTheMember()
    {
        var handler = new StubHandler(r => r.RequestUri!.AbsoluteUri switch
        {
            DiscordOAuthClient.TokenEndpoint => Json(TokenJson),
            DiscordOAuthClient.CurrentUserEndpoint => Json(UserJson),
            _ => Json("""{"nick":"Annie","avatar":null,"roles":[]}"""),
        });

        var identity = await Client(handler, guildOnly: true).GetIdentityAsync("the-code");

        Assert.Equal("Annie", identity!.DisplayName);
        Assert.Equal(DiscordOAuthClient.GuildMemberEndpoint(TestAuth.GuildId), handler.Requests[2].Request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GuildOnly_NotOnTheServer_IsAnIdentityWithoutMember()
    {
        var handler = new StubHandler(r => r.RequestUri!.AbsoluteUri switch
        {
            DiscordOAuthClient.TokenEndpoint => Json(TokenJson),
            DiscordOAuthClient.CurrentUserEndpoint => Json(UserJson),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        });

        var identity = await Client(handler, guildOnly: true).GetIdentityAsync("the-code");

        Assert.NotNull(identity);
        Assert.Null(identity.Member);
    }

    [Fact]
    public async Task GuildOnly_MemberLookupFailing_IsNoIdentity()
    {
        var handler = new StubHandler(r => r.RequestUri!.AbsoluteUri switch
        {
            DiscordOAuthClient.TokenEndpoint => Json(TokenJson),
            DiscordOAuthClient.CurrentUserEndpoint => Json(UserJson),
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError),
        });

        Assert.Null(await Client(handler, guildOnly: true).GetIdentityAsync("the-code"));
    }

    [Fact]
    public async Task GetUser_ExchangesTheCode_ThenReadsTheUser()
    {
        var handler = new StubHandler(r => r.RequestUri!.AbsoluteUri == DiscordOAuthClient.TokenEndpoint ? Json(TokenJson) : Json(UserJson));

        var identity = await Client(handler).GetIdentityAsync("the-code");

        Assert.Equal(TestAuth.Ann, identity!.User);
        Assert.Null(identity.Member);
        Assert.Equal(2, handler.Requests.Count);
        var (token, form) = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, token.Method);
        Assert.Equal("Basic", token.Headers.Authorization!.Scheme);
        Assert.Equal("app-id:secret", Encoding.UTF8.GetString(Convert.FromBase64String(token.Headers.Authorization.Parameter!)));
        Assert.Contains("grant_type=authorization_code", form);
        Assert.Contains("code=the-code", form);
        var me = handler.Requests[1].Request;
        Assert.Equal(DiscordOAuthClient.CurrentUserEndpoint, me.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer at-1", me.Headers.Authorization!.ToString());
    }

    [Fact]
    public async Task RejectedCode_IsNoUser()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

        Assert.Null(await Client(handler).GetIdentityAsync("old-code"));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task DiscordDown_IsNoUser()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("down"));

        Assert.Null(await Client(handler).GetIdentityAsync("the-code"));
    }
}
