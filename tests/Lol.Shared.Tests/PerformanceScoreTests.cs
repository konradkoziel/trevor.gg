using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Lol.Shared.Tests;

public class PerformanceScoreTests
{
    private static RiotMatchParticipant Player(string puuid, int teamId, bool win, int k, int d, int a) =>
        new(puuid, "Garen", teamId, "TOP", win, k, d, a, 150, 10, 15_000, 12, 9000);

    [Fact]
    public void Support_WhoCarriedTheTeam_BeatsAKdaPadder()
    {
        // The padder went 3/0/1 and hid; the support died a lot but healed, shielded and warded the whole game
        var padder = Player("padder", 100, true, 3, 0, 1) with { TotalDamageDealtToChampions = 4_000, VisionScore = 5 };
        var support = Player("support", 100, true, 1, 7, 18) with
        {
            TotalDamageDealtToChampions = 9_000, VisionScore = 70, TotalHealsOnTeammates = 12_000, TotalDamageShieldedOnTeammates = 8_000
        };
        var team = new[] { padder, support, Player("a", 100, true, 8, 3, 6), Player("b", 100, true, 6, 4, 9), Player("c", 100, true, 4, 5, 7) };

        Assert.True(PerformanceScore.Of(support, team) > PerformanceScore.Of(padder, team));
    }

    [Fact]
    public void Tank_WhoSoakedTheDamage_IsNotTheReportCandidate()
    {
        var tank = Player("tank", 100, false, 1, 8, 6) with { TotalDamageTaken = 60_000, DamageSelfMitigated = 50_000, TimeCCingOthers = 90 };
        var team = new[]
        {
            tank, Player("a", 100, false, 2, 7, 3), Player("b", 100, false, 4, 6, 5), Player("c", 100, false, 3, 5, 4),
            Player("d", 100, false, 5, 6, 2)
        };

        var worst = team.MinBy(p => PerformanceScore.Of(p, team))!;

        Assert.NotEqual("tank", worst.Puuid);
    }

    [Fact]
    public void AnAverageTeammate_ScoresAboutOneMinusTheDeathsPenalty()
    {
        var team = Enumerable.Range(0, 5)
            .Select(i => Player($"p{i}", 100, true, 5, 5, 5) with
            {
                TotalDamageTaken = 20_000, DamageDealtToObjectives = 5_000, TotalHealsOnTeammates = 1_000
            })
            .ToList();

        Assert.Equal(1 - 0.25, PerformanceScore.Of(team[0], team), 3);
    }
}
