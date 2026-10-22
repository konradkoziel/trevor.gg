using System.Net;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging.Abstractions;
using trevor.Features.Lol.Tracking;
using trevor.Infrastructure.Web;

namespace trevor.Tests;

public class TrackedPlayerSyncTests
{
    private static readonly FakeRiot Riot = new()
    {
        AccountsByRiotId = new Dictionary<string, RiotAccount?>
        {
            ["Ann#EUNE"] = new("puuid-ann", "Ann", "EUNE"),
            ["Bob#EUNE"] = new("puuid-bob", "Bob", "EUNE"),
        },
        MatchIds = ["EUN1_9"],
    };

    private static Task Sync(FakeTrackedPlayerStore store, IReadOnlyList<LinkedPlayer>? tracked, FakeRiot? riot = null) =>
        new TrackedPlayerSync(new FakeWebApi { Tracked = tracked }, riot ?? Riot, store, NullLogger<TrackedPlayerSync>.Instance).SyncAsync();

    [Fact]
    public async Task SwitchedOnAccount_IsTracked_FromItsCurrentLastMatch()
    {
        var store = new FakeTrackedPlayerStore();

        await Sync(store, [new LinkedPlayer("Ann#EUNE", "site-puuid")]);

        Assert.Equal(new TrackedPlayer("puuid-ann", "Ann#EUNE", "EUN1_9", FromWebsite: true), Assert.Single(store.Players));
    }

    [Fact]
    public async Task AlreadyTracked_KeepsItsLastMatch()
    {
        var store = new FakeTrackedPlayerStore(new TrackedPlayer("puuid-ann", "Ann#EUNE", "EUN1_1", FromWebsite: true));

        await Sync(store, [new LinkedPlayer("ann#eune", "site-puuid")]);

        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-ann"));
    }

    [Fact]
    public async Task SwitchedOffAccount_IsRemoved_ButPlayersFromTrackStay()
    {
        var store = new FakeTrackedPlayerStore(
            new TrackedPlayer("puuid-ann", "Ann#EUNE", "EUN1_1", FromWebsite: true),
            new TrackedPlayer("puuid-bob", "Bob#EUNE", "EUN1_1"));

        await Sync(store, []);

        Assert.Equal(["puuid-bob"], store.Players.Select(p => p.Puuid));
    }

    [Fact]
    public async Task SiteThatCantAnswer_ChangesNothing()
    {
        var store = new FakeTrackedPlayerStore(new TrackedPlayer("puuid-ann", "Ann#EUNE", "EUN1_1", FromWebsite: true));

        await Sync(store, null);

        Assert.Single(store.Players);
    }

    [Fact]
    public async Task UnknownOrBrokenAccounts_AreSkipped()
    {
        var store = new FakeTrackedPlayerStore();

        await Sync(store, [new LinkedPlayer("Gone#EUNE", "x"), new LinkedPlayer("no tag", "y"), new LinkedPlayer("Bob#EUNE", "z")]);

        Assert.Equal(["puuid-bob"], store.Players.Select(p => p.Puuid));
    }

    [Fact]
    public async Task RiotError_SkipsTheRun_ForThatPlayer()
    {
        var store = new FakeTrackedPlayerStore();

        await Sync(store, [new LinkedPlayer("Ann#EUNE", "x")], new FakeRiot { Error = new RiotApiException(HttpStatusCode.InternalServerError) });

        Assert.Empty(store.Players);
    }

    [Fact]
    public async Task FullList_TakesNoMore()
    {
        var store = new FakeTrackedPlayerStore([.. Enumerable.Range(0, TrackCommand.MaxTrackedPlayers)
            .Select(i => new TrackedPlayer($"p{i}", $"P{i}#EUNE", null))]);

        await Sync(store, [new LinkedPlayer("Ann#EUNE", "x")]);

        Assert.DoesNotContain(store.Players, p => p.Puuid == "puuid-ann");
    }
}
