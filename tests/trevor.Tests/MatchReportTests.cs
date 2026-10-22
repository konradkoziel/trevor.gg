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
    public void Message_ShowsPlayersMvpAndFeeder_WithoutVerdict()
    {
        var report = MatchReport.For(Match(), [Ann, Bob])!;

        var embed = Assert.Single(MatchReportMessage.Format(report, "https://icons.test/Thresh.png").Embeds!);

        Assert.Equal("❌ Przegrana · Ranked Solo/Duo · 30 min", embed.Title);
        var fields = embed.Fields!;
        Assert.Equal(["Ann#EUNE", "Bob#EUNE", "🏆 MVP", "🚨 Do reportu"], fields.Select(f => f.Name));
        Assert.Equal("Yasuo (Mid)\n**2/9/3**", fields[0].Value);
        Assert.True(fields[0].Inline);
        Assert.Equal("**Bob#EUNE** — Thresh (Support) · 7/2/10", fields[2].Value);
        Assert.Equal("**Random#EUNE** — Amumu (Jungle) · 0/10/2", fields[3].Value);
        Assert.Equal("https://icons.test/Thresh.png", embed.Thumbnail!.Url);
        Assert.Null(embed.Url);
    }

    [Fact]
    public void Message_LinksToTheMatchOnTheWebsite()
    {
        const string url = "https://site.test/mecz/EUN1_1";

        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(Match(), [Ann])!, null, url).Embeds!);

        Assert.Equal(url, embed.Url);
        Assert.Contains($"]({url})", embed.Fields!.Last().Value);
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

        Assert.EndsWith("swój, wstyd podwójny", Assert.Single(embed.Fields!, f => f.Name == "🚨 Do reportu").Value);
        Assert.Null(embed.Thumbnail);
    }
}
