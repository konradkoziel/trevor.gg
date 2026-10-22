using System.Net;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.Builds;
using Web.Api.Features.Players;
using Web.Api.Features.RiotAccount;
using Web.Api.Features.Stats;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;
using static Web.Api.Tests.StatsTestData;

namespace Web.Api.Tests;

public class PlayerProfileTests
{
    private static readonly Champion[] Champions =
    [
        new("Yasuo", "Yasuo", "Pokutnik", "https://icons.test/Yasuo.png", Key: 157),
        new("Ahri", "Ahri", "Lisica", "https://icons.test/Ahri.png", Key: 103),
    ];

    private readonly SessionTokens _tokens = TestAuth.Tokens();
    private readonly FakeRiotLinkStore _links = new();
    private readonly FakeBuildStore _builds = new();

    private async Task<IActionResult> Run(FakeRiot riot, string name, string tag, bool loggedIn = true)
    {
        var context = new DefaultHttpContext();
        if (loggedIn)
            context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(new SessionUser("9", "Bob", ""))}";
        var dataDragon = new FakeDataDragon(Champions);
        var history = new MatchHistory(riot, new FakeMatchHistoryStore(), new FakeTimeProvider(Start), TestStats.Options());
        var profiles = new PlayerProfiles(_links, riot, dataDragon, new StatsReport(riot, history, dataDragon), _builds);
        return await new PlayerProfileFunction(_tokens, riot, profiles).Run(context.Request, name, tag, CancellationToken.None);
    }

    [Fact]
    public async Task LinkedPlayer_GetsRankMasteriesStatsAndBuilds()
    {
        _links.Links["1"] = new RiotLink("1", Puuid, "Ann#EUNE", Start);
        var riot = new FakeRiot { Summoner = new RiotSummoner(7, 321), MatchIds = ["EUN1_1"] };
        riot.Matches["EUN1_1"] = RiotMatchOf("Yasuo", true);
        riot.Masteries[Puuid] = [new ChampionMastery(103, 10, 50_000), new ChampionMastery(157, 30, 400_000), new ChampionMastery(999, 5, 900_000)];
        riot.Entries[Puuid] = [new LeagueEntry(LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61)];
        _builds.Builds["b1"] = new StoredBuild("b1", "1", "Ann", Start, TestBuilds.Valid());
        _builds.Builds["b2"] = new StoredBuild("b2", "2", "Bob", Start, TestBuilds.Valid());

        var profile = Assert.IsType<PlayerProfile>(Assert.IsType<OkObjectResult>(await Run(riot, "ann", "eune")).Value);

        Assert.Equal("Ann#EUNE", profile.RiotId);
        Assert.Equal(321, profile.Level);
        // Unknown champion ids (not in Data Dragon yet) are skipped, the rest by points
        Assert.Equal(["Yasuo", "Ahri"], profile.TopChampions.Select(c => c.Name));
        Assert.Equal("Gold II", Assert.Single(profile.Stats.Ranks!).Label);
        Assert.Equal(1, profile.Stats.Games);
        Assert.Equal("b1", Assert.Single(profile.Builds).Id);
    }

    [Fact]
    public async Task TopChampions_AreCapped()
    {
        _links.Links["1"] = new RiotLink("1", Puuid, "Ann#EUNE", Start);
        var riot = new FakeRiot();
        riot.Masteries[Puuid] = [.. Enumerable.Range(0, PlayerProfiles.TopChampionCount + 2).Select(i => new ChampionMastery(157, 1, i))];

        var profile = Assert.IsType<PlayerProfile>(Assert.IsType<OkObjectResult>(await Run(riot, "Ann", "EUNE")).Value);

        Assert.Equal(PlayerProfiles.TopChampionCount, profile.TopChampions.Count);
    }

    [Fact]
    public async Task UnlinkedPlayer_Is404()
    {
        var result = await Run(new FakeRiot(), "Nick", "EUNE");

        Assert.Equal(PlayerProfileFunction.NotFound, Assert.IsType<ApiError>(Assert.IsType<NotFoundObjectResult>(result).Value).Error);
    }

    [Fact]
    public async Task RiotDown_Is503()
    {
        _links.Links["1"] = new RiotLink("1", Puuid, "Ann#EUNE", Start);

        var result = await Run(new FakeRiot { Error = new RiotApiException(HttpStatusCode.TooManyRequests) }, "Ann", "EUNE");

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task NeedsLogin() => Assert.IsType<UnauthorizedResult>(await Run(new FakeRiot(), "Ann", "EUNE", loggedIn: false));
}
