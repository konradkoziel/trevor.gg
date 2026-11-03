using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Lol.Shared.Riot.Models;
using trevor.Features.Lol.Tracking;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Web;
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
        FakeTrackedPlayerStore store, FakeRiot riot, FakeDiscordApi discord, string? webhook = Webhook,
        FakeReportedMatchStore? reported = null, FakeDataDragon? dataDragon = null, string? siteUrl = null) =>
        new MatchTracker(store, reported ?? new FakeReportedMatchStore(), riot, dataDragon ?? new FakeDataDragon(), discord,
                Options.Create(new DiscordOptions { MatchesWebhookUrl = webhook }),
                Options.Create(new WebApiOptions { SiteUrl = siteUrl }), NullLogger<MatchTracker>.Instance)
            .PostNewMatchesAsync();

    [Fact]
    public async Task Report_LinksToTheMatchPage()
    {
        var discord = new FakeDiscordApi();

        await Run(new FakeTrackedPlayerStore(Ann()), RiotWith(Match()), discord, siteUrl: "https://site.test");

        Assert.Equal("https://site.test/mecz/EUN1_1", Assert.Single(Assert.Single(discord.Webhooks).Message.Embeds!).Url);
    }

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
        Assert.Contains(Assert.Single(message.Embeds!).Fields!, f => f.Name == "Ann#EUNE");
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-1"));
    }

    [Fact]
    public async Task FriendsInTheSameMatch_GetOneReport()
    {
        var store = new FakeTrackedPlayerStore(Ann(), Bob());
        var discord = new FakeDiscordApi();

        await Run(store, RiotWith(Match()), discord);

        var embed = Assert.Single(Assert.Single(discord.Webhooks).Message.Embeds!);
        Assert.Contains(embed.Fields!, f => f.Name == "Ann#EUNE");
        Assert.Contains(embed.Fields!, f => f.Name == "Bob#EUNE");
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-2"));
    }

    // Riot can list a shared match for one friend minutes before the other
    [Fact]
    public async Task FriendWhoseMatchShowsUpLater_DoesNotGetASecondReport()
    {
        var store = new FakeTrackedPlayerStore(Ann(), Bob());
        var latest = new Dictionary<string, IReadOnlyList<string>> { ["puuid-1"] = ["EUN1_1"], ["puuid-2"] = ["EUN1_0"] };
        var riot = new FakeRiot { MatchIdsByPuuid = latest, MatchesById = new Dictionary<string, RiotMatch> { ["EUN1_1"] = Match() } };
        var reported = new FakeReportedMatchStore();
        var discord = new FakeDiscordApi();

        Assert.Equal(1, await Run(store, riot, discord, reported: reported));
        latest["puuid-2"] = ["EUN1_1"];
        Assert.Equal(0, await Run(store, riot, discord, reported: reported));

        var embed = Assert.Single(Assert.Single(discord.Webhooks).Message.Embeds!);
        Assert.Contains(embed.Fields!, f => f.Name == "Bob#EUNE");
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-2"));
    }

    [Fact]
    public async Task AlreadyReportedMatch_IsNotPostedAgain()
    {
        var reported = new FakeReportedMatchStore();
        reported.MatchIds.Add("EUN1_1");
        var discord = new FakeDiscordApi();

        Assert.Equal(0, await Run(new FakeTrackedPlayerStore(Ann()), RiotWith(Match()), discord, reported: reported));
        Assert.Empty(discord.Webhooks);
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
        var reported = new FakeReportedMatchStore();
        var discord = new FakeDiscordApi();

        Assert.Equal(0, await Run(store, RiotWith(Match(duration: 200)), discord, reported: reported));
        Assert.Empty(discord.Webhooks);
        Assert.Equal("EUN1_1", store.LastMatchOf("puuid-1"));
        Assert.Contains("EUN1_1", reported.MatchIds);
    }

    [Fact]
    public async Task FailedPost_IsRetriedOnTheNextRun()
    {
        var store = new FakeTrackedPlayerStore(Ann());
        var riot = RiotWith(Match());
        var discord = new FakeDiscordApi { Fail = true };

        var reported = new FakeReportedMatchStore();
        Assert.Equal(0, await Run(store, riot, discord, reported: reported));
        Assert.Equal("EUN1_0", store.LastMatchOf("puuid-1"));
        Assert.Empty(reported.MatchIds);

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
    public async Task Card_ShowsMvpAndFeeder_WithMvpIcon()
    {
        var discord = new FakeDiscordApi();
        var dataDragon = new FakeDataDragon();

        await Run(new FakeTrackedPlayerStore(Ann()), RiotWith(Match()), discord, dataDragon: dataDragon);

        var embed = Assert.Single(discord.Webhooks).Message.Embeds![0];
        Assert.Equal(["Ann#EUNE", "🏆 MVP", "🚨 Do reportu"], embed.Fields!.Select(f => f.Name));
        Assert.Equal("Thresh", dataDragon.RequestedChampion);
        Assert.NotNull(embed.Thumbnail);
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
