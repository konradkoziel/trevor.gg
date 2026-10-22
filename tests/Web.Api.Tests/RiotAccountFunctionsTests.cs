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

    private async Task<IActionResult> Link(FakeRiot riot, string json = """{ "riotId": "Player#EUNE" }""", SessionUser? user = null) =>
        await new LinkRiotAccountFunction(_tokens, riot, _store, _time).Run(await Request(user ?? Ann, json), CancellationToken.None);

    private static string ErrorOf(IActionResult result) => Assert.IsType<ApiError>(Assert.IsAssignableFrom<ObjectResult>(result).Value).Error;

    [Fact]
    public async Task Link_SavesTheAccount_WithRiotsSpelling()
    {
        var riot = new FakeRiot { Account = new RiotAccount("puuid-1", "Player", "EUNE") };

        var result = await Link(riot, """{ "riotId": "player#eune" }""");

        Assert.Equal("Player#EUNE", Assert.IsType<RiotAccountView>(Assert.IsType<OkObjectResult>(result).Value).LinkedRiotId);
        Assert.Equal(new RiotLink("1", "puuid-1", "Player#EUNE", Now), _store.Links["1"]);
    }

    [Fact]
    public async Task Link_ReplacesTheUsersEarlierAccount()
    {
        _store.Links["1"] = new RiotLink("1", "old", "Old#EUNE", Now);

        await Link(new FakeRiot());

        Assert.Equal("puuid-1", Assert.Single(_store.Links).Value.Puuid);
    }

    [Fact]
    public async Task Link_SameAccountAgain_IsFine()
    {
        _store.Links["1"] = new RiotLink("1", "puuid-1", "Player#EUNE", Now);

        Assert.IsType<OkObjectResult>(await Link(new FakeRiot()));
    }

    [Fact]
    public async Task Link_AccountOfSomeoneElse_Is409()
    {
        _store.Links["2"] = new RiotLink("2", "puuid-1", "Player#EUNE", Now);

        var result = await Link(new FakeRiot());

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(RiotAccountMessages.AlreadyLinked, ErrorOf(result));
        Assert.False(_store.Links.ContainsKey("1"));
    }

    [Theory]
    [InlineData("""{ "riotId": "Player" }""")]
    [InlineData("""{ }""")]
    [InlineData("not json")]
    public async Task Link_BadRiotId_Is400(string json) =>
        Assert.Equal(RiotAccountMessages.BadRiotId, ErrorOf(await Link(new FakeRiot(), json)));

    [Fact]
    public async Task Link_UnknownAccount_Is400() =>
        Assert.Equal(RiotAccountMessages.NoSuchAccount, ErrorOf(await Link(new FakeRiot { Account = null })));

    [Fact]
    public async Task Link_AccountThatNeverPlayed_Is400() =>
        Assert.Equal(RiotAccountMessages.NeverPlayed, ErrorOf(await Link(new FakeRiot { Summoner = null })));

    [Fact]
    public async Task Link_WithoutRiotKey_Is503()
    {
        var result = await Link(new FakeRiot { IsConfigured = false });

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(RiotAccountMessages.RiotNotConfigured, ErrorOf(result));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Link_RejectedRiotKey_SaysSo(HttpStatusCode status)
    {
        var result = await Link(new FakeRiot { Error = new RiotApiException(status) });

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(RiotAccountMessages.RiotKeyRejected, ErrorOf(result));
    }

    [Fact]
    public async Task Link_RiotDown_Is503()
    {
        var result = await Link(new FakeRiot { Error = new RiotApiException(HttpStatusCode.ServiceUnavailable) });

        Assert.Equal(RiotAccountMessages.RiotUnavailable, ErrorOf(result));
    }

    [Fact]
    public async Task Link_NeedsLogin()
    {
        var result = await new LinkRiotAccountFunction(_tokens, new FakeRiot(), _store, _time)
            .Run(await Request(null, """{ "riotId": "Player#EUNE" }"""), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Get_ShowsTheLink()
    {
        _store.Links["1"] = new RiotLink("1", "p", "Player#EUNE", Now);

        var view = Assert.IsType<OkObjectResult>(await new GetRiotAccountFunction(_tokens, _store).Run(await Request(Ann), CancellationToken.None));

        Assert.Equal(new RiotAccountView("Player#EUNE"), view.Value);
    }

    [Fact]
    public async Task Unlink_RemovesTheLink()
    {
        _store.Links["1"] = new RiotLink("1", "p", "Player#EUNE", Now);

        var result = await new UnlinkRiotAccountFunction(_tokens, _store).Run(await Request(Ann), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(_store.Links);
    }
}
