using System.Net;
using System.Text;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class RiotAccountFunctionsTests
{
    private static readonly SessionUser Ann = new("1", "Ann", "");
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly SessionTokens _tokens = TestAuth.Tokens();
    private readonly FakeRiotLinkStore _store = new();
    private readonly FakeTimeProvider _time = new(Now);
    private readonly FakeDataDragon _dataDragon = new(TestBuilds.Champions);

    private async Task<HttpRequest> Request(SessionUser? user, string? json = null)
    {
        var context = new DefaultHttpContext();
        if (user is not null)
            context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(user)}";
        if (json is not null)
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Request.ContentType = "application/json";
        }
        return context.Request;
    }

    private async Task<IActionResult> Start(FakeRiot riot, string json = """{ "riotId": "Player#EUNE" }""", SessionUser? user = null) =>
        await new StartRiotLinkFunction(_tokens, riot, _store, _dataDragon, _time, new Random(1))
            .Run(await Request(user ?? Ann, json), CancellationToken.None);

    private async Task<IActionResult> Verify(FakeRiot riot) =>
        await new VerifyRiotLinkFunction(_tokens, riot, _store, _time).Run(await Request(Ann), CancellationToken.None);

    private static string ErrorOf(IActionResult result) => Assert.IsType<ApiError>(Assert.IsAssignableFrom<ObjectResult>(result).Value).Error;

    [Fact]
    public async Task Start_PicksADefaultIcon_OtherThanTheCurrentOne()
    {
        var result = await Start(new FakeRiot { Summoner = new RiotSummoner(5, 100) });

        var view = Assert.IsType<RiotChallengeView>(Assert.IsType<OkObjectResult>(result).Value);
        var saved = _store.Challenges["1"];
        Assert.Equal("Player#EUNE", view.RiotId);
        Assert.Equal(saved.IconId, view.IconId);
        Assert.InRange(view.IconId, 0, 28);
        Assert.NotEqual(5, view.IconId);
        Assert.Equal($"https://icons.test/profile/{view.IconId}.png", view.IconUrl);
        Assert.Equal(Now + StartRiotLinkFunction.ChallengeLifetime, saved.ExpiresAt);
        Assert.Equal("puuid-1", saved.Puuid);
    }

    [Theory]
    [InlineData("""{ "riotId": "Player" }""")]
    [InlineData("""{ }""")]
    [InlineData("not json")]
    public async Task Start_BadRiotId_Is400(string json) =>
        Assert.Equal(RiotAccountMessages.BadRiotId, ErrorOf(await Start(new FakeRiot(), json)));

    [Fact]
    public async Task Start_UnknownAccount_Is400() =>
        Assert.Equal(RiotAccountMessages.NoSuchAccount, ErrorOf(await Start(new FakeRiot { Account = null })));

    [Fact]
    public async Task Start_AccountThatNeverPlayed_Is400() =>
        Assert.Equal(RiotAccountMessages.NeverPlayed, ErrorOf(await Start(new FakeRiot { Summoner = null })));

    [Fact]
    public async Task Start_WithoutRiotKey_Is503()
    {
        var result = await Start(new FakeRiot { IsConfigured = false });

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(RiotAccountMessages.RiotNotConfigured, ErrorOf(result));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Start_RejectedRiotKey_SaysSo(HttpStatusCode status)
    {
        var result = await Start(new FakeRiot { Error = new RiotApiException(status) });

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(RiotAccountMessages.RiotKeyRejected, ErrorOf(result));
    }

    [Fact]
    public async Task Start_RiotDown_Is503()
    {
        var result = await Start(new FakeRiot { Error = new RiotApiException(HttpStatusCode.ServiceUnavailable) });

        Assert.Equal(RiotAccountMessages.RiotUnavailable, ErrorOf(result));
    }

    [Fact]
    public async Task Start_NeedsLogin()
    {
        var result = await new StartRiotLinkFunction(_tokens, new FakeRiot(), _store, _dataDragon, _time, Random.Shared)
            .Run(await Request(null, """{ "riotId": "Player#EUNE" }"""), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Verify_WithTheRightIcon_LinksTheAccount()
    {
        var riot = new FakeRiot();
        await Start(riot);
        riot.Summoner = new RiotSummoner(_store.Challenges["1"].IconId, 100);

        var result = await Verify(riot);

        Assert.Equal("Player#EUNE", Assert.IsType<RiotAccountView>(Assert.IsType<OkObjectResult>(result).Value).LinkedRiotId);
        Assert.Equal("puuid-1", _store.Links["1"].Puuid);
        Assert.Empty(_store.Challenges);
    }

    [Fact]
    public async Task Verify_WithTheOldIcon_Fails()
    {
        var riot = new FakeRiot();
        await Start(riot);

        Assert.Equal(RiotAccountMessages.WrongIcon, ErrorOf(await Verify(riot)));
        Assert.Empty(_store.Links);
    }

    [Fact]
    public async Task Verify_WithoutChallenge_Fails() =>
        Assert.Equal(RiotAccountMessages.NoChallenge, ErrorOf(await Verify(new FakeRiot())));

    [Fact]
    public async Task Verify_AfterTheChallengeExpired_Fails()
    {
        var riot = new FakeRiot();
        await Start(riot);
        riot.Summoner = new RiotSummoner(_store.Challenges["1"].IconId, 100);
        _time.Advance(StartRiotLinkFunction.ChallengeLifetime + TimeSpan.FromSeconds(1));

        Assert.Equal(RiotAccountMessages.ChallengeExpired, ErrorOf(await Verify(riot)));
    }

    [Fact]
    public async Task Get_ShowsTheLink_AndOnlyALiveChallenge()
    {
        _store.Links["1"] = new RiotLink("1", "p", "Player#EUNE", Now);
        _store.Challenges["1"] = new RiotLinkChallenge("1", "p2", "Other#EUNE", 3, Now.AddMinutes(1));
        var get = new GetRiotAccountFunction(_tokens, _store, _dataDragon, _time);

        var live = (RiotAccountView)Assert.IsType<OkObjectResult>(await get.Run(await Request(Ann), CancellationToken.None)).Value!;
        _time.Advance(TimeSpan.FromMinutes(2));
        var expired = (RiotAccountView)Assert.IsType<OkObjectResult>(await get.Run(await Request(Ann), CancellationToken.None)).Value!;

        Assert.Equal("Player#EUNE", live.LinkedRiotId);
        Assert.Equal(3, live.Challenge!.IconId);
        Assert.Null(expired.Challenge);
    }

    [Fact]
    public async Task Unlink_RemovesEverything()
    {
        _store.Links["1"] = new RiotLink("1", "p", "Player#EUNE", Now);

        var result = await new UnlinkRiotAccountFunction(_tokens, _store).Run(await Request(Ann), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(_store.Links);
    }

    [Fact]
    public void VerificationIcon_IsNeverTheCurrentOne()
    {
        var random = new Random(42);
        for (var i = 0; i < 500; i++)
        {
            var icon = VerificationIcons.Pick(7, random);
            Assert.InRange(icon, 0, 28);
            Assert.NotEqual(7, icon);
        }
    }
}
