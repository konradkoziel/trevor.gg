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
        var history = new MatchHistory(riot, new FakeMatchHistoryStore(), new FakeTimeProvider(Start));
        return await new MyStatsFunction(_tokens, _links, riot, history, new FakeDataDragon(TestBuilds.Champions))
            .Run(context.Request, CancellationToken.None);
    }

    [Fact]
    public async Task LinkedAccount_GetsItsStats()
    {
        _links.Links["1"] = new RiotLink("1", Puuid, "Player#EUNE", Start);
        var riot = new FakeRiot { MatchIds = ["EUN1_1"] };
        riot.Matches["EUN1_1"] = RiotMatchOf("Yasuo", true);

        var stats = Assert.IsType<PlayerStats>(Assert.IsType<OkObjectResult>(await Run(riot, Ann)).Value);

        Assert.Equal("Player#EUNE", stats.RiotId);
        Assert.Equal(1, stats.Games);
        Assert.Equal("Yasuo", Assert.Single(stats.Champions).ChampionName);
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
        _links.Links["1"] = new RiotLink("1", Puuid, "Player#EUNE", Start);

        var result = await Run(new FakeRiot { Error = new RiotApiException(HttpStatusCode.TooManyRequests) }, Ann);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task NeedsLogin() => Assert.IsType<UnauthorizedResult>(await Run(new FakeRiot()));
}
