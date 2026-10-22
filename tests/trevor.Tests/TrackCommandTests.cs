using Microsoft.Extensions.Options;
using trevor.Features.Lol.Common;
using trevor.Features.Lol.Tracking;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Tests;

// Player lookup and Riot errors are covered by RiotPlayerRunnerTests
public class TrackCommandTests
{
    private static async Task<string> Track(FakeTrackedPlayerStore store, FakeRiot? riot = null, string? webhook = "https://discord.test/webhook")
    {
        riot ??= new FakeRiot { MatchIds = ["EUN1_9"] };
        var command = new TrackCommand(new RiotPlayerRunner(riot, new FakeWebApi()), riot, store,
            Options.Create(new DiscordOptions { MatchesWebhookUrl = webhook }));
        return (await command.ExecuteAsync(Nick("track"))).Content!;
    }

    private static async Task<string> Untrack(FakeTrackedPlayerStore store) =>
        (await new UntrackCommand(new RiotPlayerRunner(new FakeRiot(), new FakeWebApi()), store).ExecuteAsync(Nick("untrack"))).Content!;

    private static DiscordInteraction Nick(string command) =>
        TestInteractions.Command(command, """[{ "name": "nick", "type": 3, "value": "Player#EUNE" }]""");

    [Fact]
    public async Task Track_StoresPlayer_FromTheirCurrentLastMatch()
    {
        var store = new FakeTrackedPlayerStore();

        Assert.Equal(TrackingMessage.Tracking("Player#EUNE", hasChannel: true), await Track(store));
        Assert.Equal(new TrackedPlayer("puuid-1", "Player#EUNE", "EUN1_9"), Assert.Single(store.Players));
    }

    [Fact]
    public async Task Track_PlayerWithoutMatches_StartsFromNothing()
    {
        var store = new FakeTrackedPlayerStore();

        await Track(store, new FakeRiot { MatchIds = [] });

        Assert.Null(Assert.Single(store.Players).LastMatchId);
    }

    [Fact]
    public async Task Track_AlreadyTracked_KeepsTheirLastMatch()
    {
        var store = new FakeTrackedPlayerStore(new TrackedPlayer("puuid-1", "Player#EUNE", "EUN1_1"));

        Assert.Equal(TrackingMessage.AlreadyTracked("Player#EUNE"), await Track(store));
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-1"));
    }

    [Fact]
    public async Task Track_RefusesOverTheLimit()
    {
        var store = new FakeTrackedPlayerStore([.. Enumerable.Range(0, TrackCommand.MaxTrackedPlayers)
            .Select(i => new TrackedPlayer($"other-{i}", $"Gracz{i}#EUNE", null))]);

        Assert.Equal(TrackingMessage.TooMany(TrackCommand.MaxTrackedPlayers), await Track(store));
        Assert.DoesNotContain(store.Players, p => p.Puuid == "puuid-1");
    }

    [Fact]
    public async Task Track_WithoutChannel_SaysSo()
    {
        Assert.EndsWith(TrackingMessage.NoChannel, await Track(new FakeTrackedPlayerStore(), webhook: null));
    }

    [Fact]
    public async Task Untrack_RemovesPlayer()
    {
        var store = new FakeTrackedPlayerStore(new TrackedPlayer("puuid-1", "Player#EUNE", null));

        Assert.Equal(TrackingMessage.Untracked("Player#EUNE"), await Untrack(store));
        Assert.Empty(store.Players);
    }

    [Fact]
    public async Task Untrack_PlayerSwitchedOnOnTheWebsite_IsKept()
    {
        var store = new FakeTrackedPlayerStore(new TrackedPlayer("puuid-1", "Player#EUNE", "EUN1_1", FromWebsite: true));

        Assert.Equal(TrackingMessage.TrackedOnWebsite("Player#EUNE"), await Untrack(store));
        Assert.Single(store.Players);
    }

    [Fact]
    public async Task Untrack_UnknownPlayer_SaysSo()
    {
        Assert.Equal(TrackingMessage.NotTracked("Player#EUNE"), await Untrack(new FakeTrackedPlayerStore()));
    }
}
