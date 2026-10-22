using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Web.Api.Features.Matches;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class MatchDetailTests
{
    private const string MatchId = "EUN1_123";

    private static readonly FakeCatalog Catalog = new()
    {
        Items = [new("3006", "Nagolenniki Berserkera", "", 1100, ["Boots"], true, "https://icons.test/3006.png")],
        RuneTrees =
        [
            new(8000, "Precyzja", "https://icons.test/p.png", [[new Rune(8005, "Atak z Zaskoczenia", "", "https://icons.test/8005.png")]]),
            new(8400, "Determinacja", "https://icons.test/r.png", []),
        ],
        SummonerSpells = [new("SummonerFlash", "Błysk", "", "https://icons.test/f.png", Key: 4)],
    };

    private static RiotMatchParticipant Player(string puuid, string champion, int team, bool win, int kills) =>
        new(puuid, champion, team, "MIDDLE", win, kills, 2, 3, 150, 10, 20000, 20, 12000, "Player", "EUNE",
            ChampLevel: 15, Item0: 3006, Item6: 3340, Summoner1Id: 4, Summoner2Id: 99,
            Perks: new RiotMatchPerks([new RiotPerkStyle(8000, [new RiotPerkSelection(8005)]), new RiotPerkStyle(8400, [])]));

    private static RiotMatch Match() =>
        new(new RiotMatchInfo(1800, 420, "CLASSIC",
            [Player("a", "Ahri", 200, false, 1), Player("b", "Yasuo", 100, true, 7), Player("c", "Garen", 100, true, 3)],
            1767290400000));

    private readonly SessionTokens _tokens = TestAuth.Tokens();

    private MatchDetailSource Source(FakeRiot riot, IMemoryCache? cache = null) =>
        new(riot, new FakeDataDragon(TestBuilds.Champions, championIcons: true), Catalog,
            cache ?? new MemoryCache(new MemoryCacheOptions { SizeLimit = MatchDetailSource.CacheSize }));

    private async Task<IActionResult> Run(FakeRiot riot, string matchId, bool loggedIn = true)
    {
        var context = new DefaultHttpContext();
        if (loggedIn)
            context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(new SessionUser("1", "Ann", ""))}";
        return await new MatchDetailFunction(_tokens, riot, Source(riot)).Run(context.Request, matchId, CancellationToken.None);
    }

    [Fact]
    public async Task BothTeams_BlueFirst_WithTotals()
    {
        var riot = new FakeRiot();
        riot.Matches[MatchId] = Match();

        var detail = (await Source(riot).GetAsync(MatchId))!;

        Assert.Equal([100, 200], detail.Teams.Select(t => t.TeamId));
        Assert.All(detail.Teams, t => Assert.Null(t.Placement));
        var blue = detail.Teams[0];
        Assert.True(blue.Win);
        Assert.Equal(10, blue.Kills);
        Assert.Equal(["Yasuo", "Garen"], blue.Players.Select(p => p.ChampionName));
        Assert.Equal("Ranked Solo/Duo", detail.Queue);
    }

    [Fact]
    public async Task Arena_SplitsIntoPairs_BestPlaceFirst()
    {
        // Riot gives several pairs the same TeamId; PlayerSubteamId is the pair
        RiotMatchParticipant ArenaPlayer(string puuid, int team, int pair, int place) =>
            Player(puuid, "Ahri", team, place <= 4, 1) with { PlayerSubteamId = pair, SubteamPlacement = place };
        var riot = new FakeRiot();
        riot.Matches[MatchId] = new(new RiotMatchInfo(1200, 1700, "CHERRY",
        [
            ArenaPlayer("a", 100, 1, 3), ArenaPlayer("b", 100, 1, 3),
            ArenaPlayer("c", 100, 2, 1), ArenaPlayer("d", 100, 2, 1),
            ArenaPlayer("e", 200, 3, 2), ArenaPlayer("f", 200, 3, 2),
        ]));

        var detail = (await Source(riot).GetAsync(MatchId))!;

        Assert.Equal([1, 2, 3], detail.Teams.Select(t => t.Placement));
        Assert.Equal([2, 3, 1], detail.Teams.Select(t => t.TeamId));
        Assert.All(detail.Teams, t => Assert.Equal(2, t.Players.Count));
    }

    [Fact]
    public async Task Players_GetTheirBuild()
    {
        var riot = new FakeRiot();
        riot.Matches[MatchId] = Match();

        var player = (await Source(riot).GetAsync(MatchId))!.Teams[0].Players[0];

        Assert.Equal("Player#EUNE", player.RiotId);
        Assert.Equal(160, player.CreepScore);
        Assert.Equal(7, player.Items.Count);
        Assert.Equal("Nagolenniki Berserkera", player.Items[0]!.Name);
        Assert.Null(player.Items[1]);
        // Not in the shop list (the trinket here): an icon without a name
        Assert.Equal(new MatchIcon("", "https://icons.test/item/3340.png"), player.Items[6]);
        // Unknown spell ids are left out
        Assert.Equal("Błysk", Assert.Single(player.Spells).Name);
        Assert.Equal("Atak z Zaskoczenia", player.Keystone!.Name);
        Assert.Equal("Determinacja", player.SecondaryTree!.Name);
    }

    [Fact]
    public async Task EachMatch_IsFetchedOnce()
    {
        var riot = new FakeRiot();
        riot.Matches[MatchId] = Match();
        var source = Source(riot);

        await source.GetAsync(MatchId);
        await source.GetAsync(MatchId);

        Assert.Single(riot.FetchedMatches);
    }

    [Fact]
    public async Task UnknownMatch_Is404() => Assert.IsType<NotFoundResult>(await Run(new FakeRiot(), MatchId));

    [Theory]
    [InlineData("../../me")]
    [InlineData("EUN1-123")]
    public async Task MalformedId_Is404_WithoutAskingRiot(string id)
    {
        var riot = new FakeRiot();

        Assert.IsType<NotFoundResult>(await Run(riot, id));
        Assert.Empty(riot.FetchedMatches);
    }

    [Fact]
    public async Task NeedsLogin() => Assert.IsType<UnauthorizedResult>(await Run(new FakeRiot(), MatchId, loggedIn: false));
}
