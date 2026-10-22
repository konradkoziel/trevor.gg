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

    private static RiotAccountsView ViewOf(IActionResult result) =>
        Assert.IsType<RiotAccountsView>(Assert.IsType<OkObjectResult>(result).Value);

    [Fact]
    public async Task Link_FirstAccount_IsMain_WithRiotsSpelling()
    {
        var riot = new FakeRiot { Account = new RiotAccount("puuid-1", "Player", "EUNE") };

        var view = ViewOf(await Link(riot, """{ "riotId": "player#eune" }"""));

        Assert.Equal(new RiotAccountView("puuid-1", "Player#EUNE", IsMain: true), Assert.Single(view.Accounts));
        Assert.Equal(new RiotLink("1", "puuid-1", "Player#EUNE", Now, IsMain: true), Assert.Single(_store.Accounts));
    }

    [Fact]
    public async Task Link_MoreAccounts_KeepsTheMainOne()
    {
        _store.Add(new RiotLink("1", "main", "Main#EUNE", Now, IsMain: true));

        var view = ViewOf(await Link(new FakeRiot()));

        Assert.Equal(["Main#EUNE", "Player#EUNE"], view.Accounts.Select(a => a.RiotId));
        Assert.Equal([true, false], view.Accounts.Select(a => a.IsMain));
    }

    [Fact]
    public async Task Link_SameAccountAgain_OnlyRefreshesTheName()
    {
        _store.Add(new RiotLink("1", "puuid-1", "OldName#EUNE", Now.AddDays(-1), IsMain: true));

        await Link(new FakeRiot());

        var account = Assert.Single(_store.Accounts);
        Assert.Equal(("Player#EUNE", Now.AddDays(-1), true), (account.RiotId, account.LinkedAt, account.IsMain));
    }

    [Fact]
    public async Task Link_TooManyAccounts_Is400()
    {
        for (var i = 0; i < RiotLinks.MaxAccounts; i++)
            _store.Add(new RiotLink("1", $"p{i}", $"Smurf{i}#EUNE", Now));

        Assert.Equal(RiotAccountMessages.TooManyAccounts, ErrorOf(await Link(new FakeRiot())));
    }

    [Fact]
    public async Task Link_AccountOfSomeoneElse_Is409()
    {
        _store.Add(new RiotLink("2", "puuid-1", "Player#EUNE", Now));

        var result = await Link(new FakeRiot());

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(RiotAccountMessages.AlreadyLinked, ErrorOf(result));
        Assert.DoesNotContain(_store.Accounts, a => a.DiscordId == "1");
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
    public async Task Get_ListsTheAccounts_MainFirst()
    {
        _store.Add(new RiotLink("1", "a", "Smurf#EUNE", Now));
        _store.Add(new RiotLink("1", "b", "Main#EUNE", Now.AddDays(1), IsMain: true));
        _store.Add(new RiotLink("2", "c", "Bob#EUNE", Now));

        var view = ViewOf(await new GetRiotAccountFunction(_tokens, _store).Run(await Request(Ann), CancellationToken.None));

        Assert.Equal(["Main#EUNE", "Smurf#EUNE"], view.Accounts.Select(a => a.RiotId));
    }

    [Fact]
    public void Main_WithoutAFlag_IsTheOldest()
    {
        RiotLink[] accounts = [new("1", "new", "New#EUNE", Now.AddDays(1)), new("1", "old", "Old#EUNE", Now)];

        Assert.Equal("old", RiotLinks.MainOf(accounts)!.Puuid);
    }

    private async Task<IActionResult> Unlink(string accountId) =>
        await new UnlinkRiotAccountFunction(_tokens, _store).Run(await Request(Ann), accountId, CancellationToken.None);

    [Fact]
    public async Task Unlink_TheMain_PromotesTheOldestOfTheRest()
    {
        _store.Add(new RiotLink("1", "main", "Main#EUNE", Now, IsMain: true));
        _store.Add(new RiotLink("1", "b", "B#EUNE", Now.AddDays(2)));
        _store.Add(new RiotLink("1", "c", "C#EUNE", Now.AddDays(1)));

        var view = ViewOf(await Unlink("main"));

        Assert.Equal(["C#EUNE", "B#EUNE"], view.Accounts.Select(a => a.RiotId));
        Assert.True(_store.Accounts.Single(a => a.Puuid == "c").IsMain);
    }

    [Fact]
    public async Task Unlink_SomeoneElsesAccount_Is404()
    {
        _store.Add(new RiotLink("2", "p", "Bob#EUNE", Now));

        Assert.IsType<NotFoundResult>(await Unlink("p"));
        Assert.Single(_store.Accounts);
    }

    [Fact]
    public async Task SetMain_MovesTheFlag()
    {
        _store.Add(new RiotLink("1", "a", "A#EUNE", Now, IsMain: true));
        _store.Add(new RiotLink("1", "b", "B#EUNE", Now.AddDays(1)));

        var view = ViewOf(await new SetMainRiotAccountFunction(_tokens, _store).Run(await Request(Ann), "b", CancellationToken.None));

        Assert.Equal("B#EUNE", view.Accounts[0].RiotId);
        Assert.Equal(["b"], _store.Accounts.Where(a => a.IsMain).Select(a => a.Puuid));
    }

    [Fact]
    public async Task SetMain_UnknownAccount_Is404() =>
        Assert.IsType<NotFoundResult>(await new SetMainRiotAccountFunction(_tokens, _store).Run(await Request(Ann), "x", CancellationToken.None));
}
