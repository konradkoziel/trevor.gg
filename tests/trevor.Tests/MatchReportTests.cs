using trevor.Features.Lol.Tracking;
using static trevor.Tests.TrackingTestData;

namespace trevor.Tests;

public class MatchReportTests
{
    private static readonly TrackedPlayer Ann = new("puuid-1", "Ann#EUNE", "EUN1_0");
    private static readonly TrackedPlayer Bob = new("puuid-2", "Bob#EUNE", "EUN1_0");

    [Fact]
    public void TrackedPlayers_AreListed_WithTheirStoredNames()
    {
        var report = MatchReport.For(Match(), [Ann, Bob])!;

        Assert.Equal(["Ann#EUNE", "Bob#EUNE"], report.Players.Select(p => p.Name));
        Assert.Equal("Yasuo", report.Players[0].Champion);
        Assert.Equal("Mid", report.Players[0].Role);
        Assert.Equal("Ranked Solo/Duo", report.Queue);
        Assert.False(report.Win);
    }

    [Fact]
    public void Feeder_IsTheWorstOfTheirTeam_ByRiotId()
    {
        var feeder = MatchReport.For(Match(), [Ann])!.Feeder;

        Assert.Equal("Random#EUNE", feeder.Name);
        Assert.Equal((0, 10, 2), (feeder.Kills, feeder.Deaths, feeder.Assists));
        Assert.False(feeder.IsTracked);
    }

    [Fact]
    public void TrackedPlayer_CanBeTheFeeder()
    {
        var match = Match();
        var annFeeds = match with
        {
            Info = match.Info with
            {
                Participants = [.. match.Info.Participants.Select(p => p.Puuid == "puuid-1" ? p with { Kills = 0, Deaths = 15, Assists = 1 } : p)]
            }
        };

        var feeder = MatchReport.For(annFeeds, [Ann, Bob])!.Feeder;

        Assert.Equal("Ann#EUNE", feeder.Name);
        Assert.True(feeder.IsTracked);
    }

    [Fact]
    public void Mvp_IsTheBestOfTheirTeam_TrackedPlayersIncluded()
    {
        var mvp = MatchReport.For(Match(), [Ann, Bob])!.Mvp;

        Assert.Equal("Bob#EUNE", mvp.Name);
        Assert.Equal(("Thresh", 7, 2, 10), (mvp.Champion, mvp.Kills, mvp.Deaths, mvp.Assists));
    }

    [Fact]
    public void Mvp_IgnoresTheEnemyTeam()
    {
        // enemy-2 went 15/2/4, better than anyone on Ann's team; Bob is untracked here and has no Riot ID in the data
        var mvp = MatchReport.For(Match(), [Ann])!.Mvp;

        Assert.Equal(("Thresh", 7, 2, 10), (mvp.Name, mvp.Kills, mvp.Deaths, mvp.Assists));
        Assert.False(mvp.IsTracked);
    }

    [Fact]
    public void Arena_PicksFromTheirPairOnly()
    {
        var report = MatchReport.For(ArenaMatch(), [Ann])!;

        Assert.Equal("Yasuo", report.Mvp.Champion);
        Assert.Equal("Partner#EUNE", report.Feeder.Name);
    }

    [Fact]
    public void FullPremade_BlamesTheWorstFriend()
    {
        TrackedPlayer[] premade =
        [
            Ann, Bob, new("ally-1", "Gracz1#EUNE", null), new("ally-2", "Gracz2#EUNE", null), new("ally-3", "Gracz3#EUNE", null)
        ];

        var feeder = MatchReport.For(Match(), premade)!.Feeder;

        Assert.Equal("Gracz3#EUNE", feeder.Name);
        Assert.True(feeder.IsTracked);
    }

    [Fact]
    public void TrackedPlayersOnBothTeams_PickFromTheWholeMatch()
    {
        var report = MatchReport.For(Match(bobOnEnemyTeam: true), [Ann, Bob])!;

        Assert.Equal("Nick#EUNE", report.Feeder.Name);
        Assert.Equal((15, 2, 4), (report.Mvp.Kills, report.Mvp.Deaths, report.Mvp.Assists));
    }

    [Fact]
    public void Feeder_WithoutRiotId_IsNamedByChampion()
    {
        var match = Match();
        var noName = match with { Info = match.Info with { Participants = [.. match.Info.Participants.Select(p => p with { RiotIdGameName = null })] } };

        Assert.Equal("Amumu", MatchReport.For(noName, [Ann])!.Feeder.Name);
    }

    [Fact]
    public void TrackedPlayersOnBothTeams_HaveNoCommonResult()
    {
        Assert.Null(MatchReport.For(Match(bobOnEnemyTeam: true), [Ann, Bob])!.Win);
    }

    [Fact]
    public void ShortGame_IsARemake()
    {
        Assert.True(MatchReport.For(Match(duration: 200), [Ann])!.IsRemake);
    }

    [Fact]
    public void MatchWithoutTrackedPlayers_HasNoReport()
    {
        Assert.Null(MatchReport.For(Match(), [new TrackedPlayer("someone-else", "X#EUNE", null)]));
    }

    [Fact]
    public void Message_ListsTheWholeTeam_OneLineEach_WithMvpAndFeederMarked()
    {
        var report = MatchReport.For(Match(), [Ann, Bob])!;
        var names = new Dictionary<string, string> { ["Amumu"] = "Amumu PL", ["Teemo"] = "Teemo PL" };

        var embed = Assert.Single(MatchReportMessage.Format(report, "https://icons.test/Thresh.png", championNames: names).Embeds!);

        Assert.Equal("❌ Przegrana · Ranked Solo/Duo · 30 min", embed.Title);
        Assert.Null(embed.Fields);
        var lines = embed.Description!.Split('\n');
        Assert.Equal("**Nasza drużyna**", lines[0]);
        Assert.Equal("**Ann#EUNE** · Yasuo (Mid) · 2/9/3", lines[1]);
        Assert.Equal("**Bob#EUNE** · Thresh (Support) · 7/2/10 · 🏆 MVP", lines[2]);
        Assert.Equal("Random#EUNE · Amumu PL (Jungle) · 0/10/2 · 🚨 do reportu", lines[5]);
        Assert.StartsWith("**Przeciwnicy:** Teemo PL, Garen", lines[7]);
        Assert.Equal("https://icons.test/Thresh.png", embed.Thumbnail!.Url);
        Assert.Null(embed.Url);
    }

    [Fact]
    public void Message_LinksToTheMatchOnTheWebsite()
    {
        const string url = "https://site.test/mecz/EUN1_1";

        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(Match(), [Ann])!, null, url).Embeds!);

        Assert.Equal(url, embed.Url);
        Assert.EndsWith($"]({url})", embed.Description);
    }

    [Fact]
    public void Message_ShowsThePairAndItsPlace_InArena()
    {
        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(ArenaMatch(), [Ann])!, null).Embeds!);

        var lines = embed.Description!.Split('\n');
        Assert.Equal("**Nasza para · 2. miejsce**", lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.DoesNotContain("Przeciwnicy", embed.Description);
    }

    [Fact]
    public void Message_ShowsBothTeams_WhenFriendsPlayedEachOther()
    {
        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(Match(bobOnEnemyTeam: true), [Ann, Bob])!, null).Embeds!);

        Assert.Contains("**Drużyna 1**", embed.Description);
        Assert.Contains("**Drużyna 2**", embed.Description);
        Assert.DoesNotContain("Przeciwnicy", embed.Description);
    }

    [Theory]
    [InlineData("https://site.test/", "https://site.test/mecz/EUN1_1")]
    [InlineData(null, null)]
    public void SiteLink_IsBuiltFromWebSiteUrl(string? siteUrl, string? expected) =>
        Assert.Equal(expected, trevor.Infrastructure.Web.SiteLinks.Match(siteUrl, "EUN1_1"));

    [Fact]
    public void Message_CallsOutATrackedFeeder()
    {
        var match = Match();
        TrackedPlayer[] premade =
        [
            Ann, Bob, new("ally-1", "Gracz1#EUNE", null), new("ally-2", "Gracz2#EUNE", null), new("ally-3", "Gracz3#EUNE", null)
        ];

        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(match, premade)!, null).Embeds!);

        Assert.Contains("**Gracz3#EUNE** · Amumu (Jungle) · 0/10/2 · 🚨 do reportu, swój, wstyd podwójny", embed.Description);
        Assert.Null(embed.Thumbnail);
    }
}
