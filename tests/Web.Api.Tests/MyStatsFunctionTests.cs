using System.Net;
using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.RiotAccount;
using Web.Api.Features.Stats;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;
using static Web.Api.Tests.StatsTestData;

namespace Web.Api.Tests;

public class MyStatsFunctionTests
{
    private static readonly SessionUser Ann = new("1", "Ann", "");

    private readonly SessionTokens _tokens = TestAuth.Tokens();
    private readonly FakeRiotLinkStore _links = new();

    private async Task<IActionResult> Run(FakeRiot riot, SessionUser? user = null)
    {
        var context = new DefaultHttpContext();
        if (user is not null)
            context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(user)}";
        var history = new MatchHistory(riot, new FakeMatchHistoryStore(), new FakeTimeProvider(Start), TestStats.Options());
        return await new MyStatsFunction(_tokens, _links, riot, new StatsReport(riot, history, new FakeDataDragon(TestBuilds.Champions)))
            .Run(context.Request, CancellationToken.None);
    }

    [Fact]
    public async Task LinkedAccount_GetsItsStats()
    {
        _links.Add(new RiotLink("1", Puuid, "Player#EUNE", Start));
        var riot = new FakeRiot { MatchIds = ["EUN1_1"] };
        riot.Matches["EUN1_1"] = RiotMatchOf("Yasuo", true);
        riot.Entries[Puuid] = [new Lol.Shared.Riot.Models.LeagueEntry(Lol.Shared.Riot.Models.LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61)];

        var stats = Assert.IsType<PlayerStats>(Assert.IsType<OkObjectResult>(await Run(riot, Ann)).Value);

        Assert.Equal("Player#EUNE", stats.RiotId);
        Assert.Equal(1, stats.Games);
        Assert.Equal("Yasuo", Assert.Single(stats.Champions).ChampionName);
        Assert.Equal("Gold II", Assert.Single(stats.Ranks!).Label);
    }

    [Fact]
    public async Task NotLinked_Is404WithAHint()
    {
        var result = await Run(new FakeRiot(), Ann);

        Assert.Equal(MyStatsFunction.NotLinked, Assert.IsType<ApiError>(Assert.IsType<NotFoundObjectResult>(result).Value).Error);
    }

    [Fact]
    public async Task RiotDown_Is503()
    {
        _links.Add(new RiotLink("1", Puuid, "Player#EUNE", Start));

        var result = await Run(new FakeRiot { Error = new RiotApiException(HttpStatusCode.TooManyRequests) }, Ann);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task NeedsLogin() => Assert.IsType<UnauthorizedResult>(await Run(new FakeRiot()));

    [Fact]
    public async Task AccountParameter_PicksAnotherOfTheUsersAccounts()
    {
        _links.Add(new RiotLink("1", "main-puuid", "Main#EUNE", Start, IsMain: true));
        _links.Add(new RiotLink("1", Puuid, "Smurf#EUNE", Start));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(Ann)}";
        context.Request.QueryString = QueryString.Create("account", Puuid);
        var riot = new FakeRiot();
        var history = new MatchHistory(riot, new FakeMatchHistoryStore(), new FakeTimeProvider(Start), TestStats.Options());

        var result = await new MyStatsFunction(_tokens, _links, riot, new StatsReport(riot, history, new FakeDataDragon(TestBuilds.Champions)))
            .Run(context.Request, CancellationToken.None);

        Assert.Equal("Smurf#EUNE", Assert.IsType<PlayerStats>(Assert.IsType<OkObjectResult>(result).Value).RiotId);
    }
}
