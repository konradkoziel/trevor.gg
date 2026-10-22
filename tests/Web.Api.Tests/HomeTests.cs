using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.Builds;
using Web.Api.Features.Home;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class HomeTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly Champion[] Champions =
    [
        new("Yasuo", "Yasuo", "Pokutnik", "https://icons.test/Yasuo.png", Key: 157),
        new("Ahri", "Ahri", "Lisica", "https://icons.test/Ahri.png", Key: 103),
        new("Garen", "Garen", "Potęga Demacii", "https://icons.test/Garen.png", Key: 86),
    ];

    private readonly FakeTimeProvider _time = new(Now);
    private readonly FakeRiotLinkStore _links = new();
    private readonly SessionTokens _tokens = TestAuth.Tokens();

    private async Task<HttpRequest> LoggedIn()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(new SessionUser("1", "Ann", ""))}";
        return context.Request;
    }

    [Fact]
    public async Task Rotation_MapsRiotsKeysToChampions_AndCachesForAnHour()
    {
        var riot = new FakeRiot { FreeChampionKeys = [157, 86] };
        var rotation = new FreeRotation(riot, new FakeDataDragon(Champions), _time);

        Assert.Equal(["Yasuo", "Garen"], (await rotation.ChampionsAsync())!.Select(c => c.Name));
        await rotation.ChampionsAsync();
        Assert.Equal(1, riot.FreeRotationCalls);

        _time.Advance(FreeRotation.CacheFor);
        await rotation.ChampionsAsync();
        Assert.Equal(2, riot.FreeRotationCalls);
    }

    [Fact]
    public async Task Rotation_WithoutRiotKey_Is503()
    {
        var riot = new FakeRiot { IsConfigured = false };

        var result = await new RotationFunction(riot, new FreeRotation(riot, new FakeDataDragon(Champions), _time))
            .Run(new DefaultHttpContext().Request, CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    private Leaderboard Board(FakeRiot riot) =>
        new(_links, riot, new FakeDataDragon(Champions), _time, NullLogger<Leaderboard>.Instance);

    [Fact]
    public async Task Leaderboard_SortsByMasteryPoints_WithTopChampion()
    {
        _links.Links["1"] = new RiotLink("1", "p1", "Ann#EUNE", Now);
        _links.Links["2"] = new RiotLink("2", "p2", "Bob#EUNE", Now);
        var riot = new FakeRiot { Summoner = new RiotSummoner(7, 300) };
        riot.Masteries["p1"] = [new ChampionMastery(157, 30, 400_000), new ChampionMastery(103, 10, 50_000)];
        riot.Masteries["p2"] = [new ChampionMastery(86, 50, 900_000)];
        riot.Entries["p1"] = [new LeagueEntry(LeagueEntry.FlexQueue, "SILVER", "I", 12, 10, 10)];

        var entries = await Board(riot).EntriesAsync();

        Assert.Equal(["Bob#EUNE", "Ann#EUNE"], entries.Select(e => e.RiotId));
        var ann = entries[1];
        Assert.Equal(450_000, ann.MasteryPoints);
        Assert.Equal("Yasuo", ann.TopChampionName);
        Assert.Equal(400_000, ann.TopChampionPoints);
        Assert.Equal(300, ann.Level);
        Assert.Equal("https://icons.test/profile/7.png", ann.ProfileIconUrl);
        Assert.Equal("Silver I", ann.Rank!.Label);
        Assert.Null(entries[0].Rank);
    }

    [Fact]
    public async Task Leaderboard_SkipsPlayersRiotFailsOn()
    {
        _links.Links["1"] = new RiotLink("1", "p1", "Ann#EUNE", Now);
        _links.Links["2"] = new RiotLink("2", "p2", "Bob#EUNE", Now);
        var riot = new FakeRiot();
        riot.FailingPuuids.Add("p2");

        Assert.Equal(["Ann#EUNE"], (await Board(riot).EntriesAsync()).Select(e => e.RiotId));
    }

    [Fact]
    public async Task Leaderboard_IsCached()
    {
        var board = Board(new FakeRiot());
        await board.EntriesAsync();
        _links.Links["1"] = new RiotLink("1", "p1", "Ann#EUNE", Now);

        Assert.Empty(await board.EntriesAsync());
        _time.Advance(Leaderboard.CacheFor);
        Assert.Single(await board.EntriesAsync());
    }

    [Fact]
    public async Task Leaderboard_IsForLoggedInUsers()
    {
        var riot = new FakeRiot();
        var function = new LeaderboardFunction(_tokens, riot, Board(riot));

        Assert.IsType<UnauthorizedResult>(await function.Run(new DefaultHttpContext().Request, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await function.Run(await LoggedIn(), CancellationToken.None));
    }

    [Fact]
    public async Task RecentBuilds_AreEveryonesNewest_ForLoggedInUsers()
    {
        var store = new FakeBuildStore();
        for (var i = 0; i < RecentBuildsFunction.Count + 2; i++)
            store.Builds[$"b{i}"] = new StoredBuild($"b{i}", i % 2 == 0 ? "1" : "2", i % 2 == 0 ? "Ann" : "Bob", Now.AddHours(i), TestBuilds.Valid());
        var function = new RecentBuildsFunction(_tokens, store);

        Assert.IsType<UnauthorizedResult>(await function.Run(new DefaultHttpContext().Request, CancellationToken.None));
        var builds = Assert.IsAssignableFrom<IEnumerable<RecentBuild>>(
            Assert.IsType<OkObjectResult>(await function.Run(await LoggedIn(), CancellationToken.None)).Value).ToList();
        Assert.Equal(RecentBuildsFunction.Count, builds.Count);
        Assert.Equal("b7", builds[0].Id);
        Assert.Equal("Bob", builds[0].OwnerName);
    }
}
