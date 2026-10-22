using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.Stats;
using static Web.Api.Tests.StatsTestData;

namespace Web.Api.Tests;

public class MatchHistoryTests
{
    private readonly FakeRiot _riot = new();
    private readonly FakeMatchHistoryStore _store = new();
    private readonly FakeTimeProvider _time = new(Start);

    private MatchHistory History(int games = MatchHistoryOptions.DefaultGames) => new(_riot, _store, _time, TestStats.Options(games));

    [Fact]
    public async Task FirstVisit_FetchesEveryMatch_AndStoresThem()
    {
        _riot.MatchIds = ["EUN1_2", "EUN1_1"];
        _riot.Matches["EUN1_1"] = RiotMatchOf("Yasuo", true, endMs: Start.AddHours(1).ToUnixTimeMilliseconds());
        _riot.Matches["EUN1_2"] = RiotMatchOf("Ahri", false, endMs: Start.AddHours(2).ToUnixTimeMilliseconds());

        var matches = await History().RecentAsync(Puuid);

        Assert.Equal(["EUN1_2", "EUN1_1"], matches.Select(m => m.MatchId));
        var yasuo = matches[1];
        Assert.Equal(180, yasuo.CreepScore); // minions + monsters
        Assert.Equal(Start.AddHours(1), yasuo.PlayedAt);
        Assert.Equal(2, _store.ByPuuid[Puuid].Count);
    }

    [Fact]
    public async Task LaterVisit_FetchesOnlyNewMatches()
    {
        _store.ByPuuid[Puuid] = [Played(1, "Yasuo", true, 1, 1, 1)];
        _riot.MatchIds = ["EUN1_2", "EUN1_1"];
        _riot.Matches["EUN1_2"] = RiotMatchOf("Ahri", false, endMs: Start.AddHours(2).ToUnixTimeMilliseconds());

        var matches = await History().RecentAsync(Puuid);

        Assert.Equal(["EUN1_2"], _riot.FetchedMatches);
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task Reloads_WithinTwoMinutes_DontAskRiot()
    {
        var history = History();
        await history.RecentAsync(Puuid);

        _time.Advance(TimeSpan.FromMinutes(1));
        await history.RecentAsync(Puuid);
        Assert.Equal(1, _riot.MatchIdCalls);

        _time.Advance(MatchHistory.RefreshInterval);
        await history.RecentAsync(Puuid);
        Assert.Equal(2, _riot.MatchIdCalls);
    }

    [Fact]
    public async Task MatchesThePlayerIsNotIn_AreSkipped()
    {
        _riot.MatchIds = ["EUN1_1"];
        _riot.Matches["EUN1_1"] = RiotMatchOf("Yasuo", true, puuid: "another-player");

        Assert.Empty(await History().RecentAsync(Puuid));
    }

    [Fact]
    public async Task OnlyTheLatestTwentyIds_AreShown()
    {
        _store.ByPuuid[Puuid] = [Played(1, "Yasuo", true, 1, 1, 1)];
        _riot.MatchIds = ["EUN1_9"];
        _riot.Matches["EUN1_9"] = RiotMatchOf("Ahri", false);

        var matches = await History().RecentAsync(Puuid);

        // EUN1_1 dropped out of Riot's recent list, so it's no longer "recent"
        Assert.Equal(["EUN1_9"], matches.Select(m => m.MatchId));
    }

    [Fact]
    public async Task GamesOlderThanTheLastOnes_AreDeleted()
    {
        _store.ByPuuid[Puuid] = [Played(1, "Yasuo", true, 1, 1, 1), Played(2, "Ahri", true, 1, 1, 1)];
        _riot.MatchIds = ["EUN1_3", "EUN1_2"];
        _riot.Matches["EUN1_3"] = RiotMatchOf("Zed", false, endMs: Start.AddHours(3).ToUnixTimeMilliseconds());

        await History(games: 2).RecentAsync(Puuid);

        Assert.Equal(["EUN1_2", "EUN1_3"], _store.ByPuuid[Puuid].Select(m => m.MatchId).Order());
    }

    [Fact]
    public async Task TheSettingDecidesHowManyGames()
    {
        _riot.MatchIds = ["EUN1_3", "EUN1_2", "EUN1_1"];
        foreach (var id in _riot.MatchIds)
            _riot.Matches[id] = RiotMatchOf("Zed", true);

        await History(games: 2).RecentAsync(Puuid);

        Assert.Equal(["EUN1_3", "EUN1_2"], _riot.FetchedMatches);
    }

    [Fact]
    public async Task NoIdsFromRiot_DeletesNothing()
    {
        _store.ByPuuid[Puuid] = [Played(1, "Yasuo", true, 1, 1, 1)];

        await History().RecentAsync(Puuid);

        Assert.Single(_store.ByPuuid[Puuid]);
    }
}
