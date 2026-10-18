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
    public void Feeder_IsTheLowestKdaInTheMatch_ByRiotId()
    {
        var feeder = MatchReport.For(Match(), [Ann])!.Feeder;

        Assert.Equal("Nick#EUNE", feeder.Name);
        Assert.Equal((0, 12, 1), (feeder.Kills, feeder.Deaths, feeder.Assists));
        Assert.False(feeder.IsTracked);
    }

    [Fact]
    public void Feeder_WithoutRiotId_IsNamedByChampion()
    {
        var match = Match();
        var noName = match with { Info = match.Info with { Participants = [.. match.Info.Participants.Select(p => p with { RiotIdGameName = null })] } };

        Assert.Equal("Teemo", MatchReport.For(noName, [Ann])!.Feeder.Name);
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

        var embed = Assert.Single(MatchReportMessage.Format(report, "Typowe.").Embeds!);

        Assert.Equal("❌ Przegrana · Ranked Solo/Duo · 30 min", embed.Title);
        Assert.Equal("**Ann#EUNE** — Yasuo (Mid) · 2/9/3\n**Bob#EUNE** — Thresh (Support) · 7/2/10", embed.Description);
        Assert.Equal("**Nick#EUNE** — Teemo (Top) · 0/12/1 (z drużyny wroga)", embed.Fields![0].Value);
        Assert.Equal("*Typowe.*", embed.Fields[1].Value);
    }

    [Fact]
    public void Message_CallsOutATrackedFeeder()
    {
        var match = Match();
        var annFeeds = match with { Info = match.Info with { Participants = [.. match.Info.Participants.Select(p => p.Puuid == "puuid-1" ? p with { Kills = 0, Deaths = 20, Assists = 0 } : p)] } };

        var embed = Assert.Single(MatchReportMessage.Format(MatchReport.For(annFeeds, [Ann])!, "x").Embeds!);

        Assert.EndsWith("swój, wstyd podwójny", embed.Fields![0].Value);
    }
}
