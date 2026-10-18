using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using trevor.Features.Lol.Tracking;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot.Models;
using static trevor.Tests.TrackingTestData;

namespace trevor.Tests;

public class MatchTrackerTests
{
    private const string Webhook = "https://discord.test/webhook";

    private static TrackedPlayer Ann(string? lastMatch = "EUN1_0") => new("puuid-1", "Ann#EUNE", lastMatch);
    private static TrackedPlayer Bob(string? lastMatch = "EUN1_0") => new("puuid-2", "Bob#EUNE", lastMatch);

    private static FakeRiot RiotWith(RiotMatch match, string matchId = "EUN1_1") =>
        new() { MatchIds = [matchId], MatchesById = new Dictionary<string, RiotMatch> { [matchId] = match } };

    private static Task<int> Run(
        FakeTrackedPlayerStore store, FakeRiot riot, FakeDiscordApi discord, FakeAi? ai = null, string? webhook = Webhook) =>
        new MatchTracker(store, riot, ai ?? new FakeAi("Komentarz."), discord,
                Options.Create(new DiscordOptions { MatchesWebhookUrl = webhook }), NullLogger<MatchTracker>.Instance)
            .PostNewMatchesAsync();

    [Fact]
    public async Task NewMatch_IsPostedOnce_AndMarkedAsSeen()
    {
        var store = new FakeTrackedPlayerStore(Ann());
        var discord = new FakeDiscordApi();
        var riot = RiotWith(Match());

        Assert.Equal(1, await Run(store, riot, discord));
        Assert.Equal(0, await Run(store, riot, discord));

        var (url, message) = Assert.Single(discord.Webhooks);
        Assert.Equal(Webhook, url);
        Assert.Contains("Ann#EUNE", Assert.Single(message.Embeds!).Description);
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-1"));
    }

    [Fact]
    public async Task FriendsInTheSameMatch_GetOneReport()
    {
        var store = new FakeTrackedPlayerStore(Ann(), Bob());
        var discord = new FakeDiscordApi();

        await Run(store, RiotWith(Match()), discord);

        var embed = Assert.Single(Assert.Single(discord.Webhooks).Message.Embeds!);
        Assert.Contains("Ann#EUNE", embed.Description);
        Assert.Contains("Bob#EUNE", embed.Description);
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-2"));
    }

    [Fact]
    public async Task AlreadySeenMatch_IsNotPosted()
    {
        var discord = new FakeDiscordApi();

        Assert.Equal(0, await Run(new FakeTrackedPlayerStore(Ann("EUN1_1")), RiotWith(Match()), discord));
        Assert.Empty(discord.Webhooks);
    }

    [Fact]
    public async Task Remake_IsSkipped_ButMarkedAsSeen()
    {
        var store = new FakeTrackedPlayerStore(Ann());
        var discord = new FakeDiscordApi();

        Assert.Equal(0, await Run(store, RiotWith(Match(duration: 200)), discord));
        Assert.Empty(discord.Webhooks);
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-1"));
    }

    [Fact]
    public async Task FailedPost_IsRetriedOnTheNextRun()
    {
        var store = new FakeTrackedPlayerStore(Ann());
        var riot = RiotWith(Match());
        var discord = new FakeDiscordApi { Fail = true };

        Assert.Equal(0, await Run(store, riot, discord));
        Assert.Equal("EUN1_0", store.LastMatchOf("puuid-1"));

        discord.Fail = false;
        Assert.Equal(1, await Run(store, riot, discord));
    }

    [Fact]
    public async Task RiotErrorForOnePlayer_DoesNotStopTheOthers()
    {
        var store = new FakeTrackedPlayerStore(Ann(), Bob());
        var riot = RiotWith(Match());
        riot.FailingPuuids.Add("puuid-1");
        var discord = new FakeDiscordApi();

        Assert.Equal(1, await Run(store, riot, discord));
        Assert.Equal("EUN1_0", store.LastMatchOf("puuid-1"));
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-2"));
    }

    [Fact]
    public async Task AiGetsTheReport_AndFallsBackWhenUnavailable()
    {
        var ai = new FakeAi(reply: null);
        var discord = new FakeDiscordApi();

        await Run(new FakeTrackedPlayerStore(Ann()), RiotWith(Match()), discord, ai);

        Assert.Equal(TrevorPrompts.MatchSummary, ai.LastInstruction);
        Assert.Contains("Kandydat do reportu (najgorsze KDA w meczu): Nick#EUNE", ai.LastInput);
        var verdict = Assert.Single(discord.Webhooks).Message.Embeds![0].Fields![^1].Value;
        Assert.Equal("*Przegrana. Nikt nie jest zaskoczony.*", verdict);
    }

    [Fact]
    public async Task WithoutChannel_NothingIsChecked()
    {
        var store = new FakeTrackedPlayerStore(Ann());
        var discord = new FakeDiscordApi();

        Assert.Equal(0, await Run(store, RiotWith(Match()), discord, webhook: null));
        Assert.Empty(discord.Webhooks);
        Assert.Equal("EUN1_0", store.LastMatchOf("puuid-1"));
    }
}
