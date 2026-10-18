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
    public void Feeder_IsTheWorstRandomOnTheirTeam_ByRiotId()
    {
        var feeder = MatchReport.For(Match(), [Ann])!.Feeder;

        Assert.Equal("Random#EUNE", feeder.Name);
        Assert.Equal((0, 10, 2), (feeder.Kills, feeder.Deaths, feeder.Assists));
        Assert.False(feeder.IsTracked);
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
    public void TrackedPlayersOnBothTeams_BlameTheWorstOfTheMatch()
    {
        Assert.Equal("Nick#EUNE", MatchReport.For(Match(bobOnEnemyTeam: true), [Ann, Bob])!.Feeder.Name);
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
    public void Message_ShowsPlayersFeederAndComment()
    {
        var report = MatchReport.For(Match(), [Ann, Bob])!;

        var embed = Assert.Single(MatchReportMessage.Format(report, "Typowe.", "https://icons.test/Amumu.png").Embeds!);

        Assert.Equal("❌ Przegrana · Ranked Solo/Duo · 30 min", embed.Title);
        var fields = embed.Fields!;
        Assert.Equal(["Ann#EUNE", "Bob#EUNE", "🚨 Do reportu", "Werdykt Trevora"], fields.Select(f => f.Name));
        Assert.Equal("Yasuo (Mid)\n**2/9/3**", fields[0].Value);
        Assert.True(fields[0].Inline);
        Assert.Equal("**Random#EUNE** — Amumu (Jungle) · 0/10/2", fields[2].Value);
        Assert.Equal("*Typowe.*", fields[3].Value);
        Assert.Equal("https://icons.test/Amumu.png", embed.Thumbnail!.Url);
    }

    [Fact]
    public void Message_CallsOutATrackedFeeder()
    {
        var match = Match();
        TrackedPlayer[] premade =
        [
            Ann, Bob, new("ally-1", "Gracz1#EUNE", null), new("ally-2", "Gracz2#EUNE", null), new("ally-3", "Gracz3#EUNE", null)
        ];

        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(match, premade)!, "x", null).Embeds!);

        Assert.EndsWith("swój, wstyd podwójny", Assert.Single(embed.Fields!, f => f.Name == "🚨 Do reportu").Value);
        Assert.Null(embed.Thumbnail);
    }
}
