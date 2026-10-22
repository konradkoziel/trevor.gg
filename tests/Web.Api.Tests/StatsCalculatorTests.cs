using Web.Api.Features.Stats;
using static Web.Api.Tests.StatsTestData;

namespace Web.Api.Tests;

public class StatsCalculatorTests
{
    private static readonly Dictionary<string, string?> Icons = new() { ["Yasuo"] = "https://icons.test/Yasuo.png" };

    [Fact]
    public void Champions_AreSortedByGames_WithRates()
    {
        PlayedMatch[] matches =
        [
            Played(1, "Yasuo", true, 10, 2, 5),
            Played(2, "Yasuo", false, 2, 8, 2),
            Played(3, "Yasuo", true, 6, 0, 6),
            Played(4, "Ahri", true, 3, 3, 9),
        ];

        var stats = StatsCalculator.Calculate("Player#EUNE", matches, Icons);

        Assert.Equal(4, stats.Games);
        Assert.Equal(75, stats.WinRate);
        Assert.Equal(["Yasuo", "Ahri"], stats.Champions.Select(c => c.ChampionName));
        var yasuo = stats.Champions[0];
        Assert.Equal(3, yasuo.Games);
        Assert.Equal(66.7, yasuo.WinRate);
        Assert.Equal(3.1, yasuo.Kda); // (18 + 13) / 10
        Assert.Equal(6, yasuo.CsPerMinute); // 540 cs / 90 min
        Assert.Equal(600, yasuo.DamagePerMinute);
        Assert.Equal("https://icons.test/Yasuo.png", yasuo.IconUrl);
        Assert.Null(stats.Champions[1].IconUrl);
    }

    [Fact]
    public void AramArenaAndRemakes_DontCount()
    {
        PlayedMatch[] matches =
        [
            Played(1, "Yasuo", true, 10, 2, 5),
            Played(2, "Yasuo", false, 20, 20, 20, queue: 450), // ARAM
            Played(3, "Yasuo", false, 0, 1, 0, seconds: 200), // remake
        ];

        var stats = StatsCalculator.Calculate("Player#EUNE", matches, Icons);

        Assert.Equal(1, stats.Games);
        Assert.Equal(100, stats.WinRate);
        // Recent shows every real game, ARAM included, but no remakes
        Assert.Equal(2, stats.Recent.Count);
    }

    [Fact]
    public void RecentGames_CarryTheirLength()
    {
        var stats = StatsCalculator.Calculate("Player#EUNE", [Played(1, "Yasuo", true, 5, 2, 5, seconds: 1860)], Icons);

        Assert.Equal(1860, Assert.Single(stats.Recent).DurationSeconds);
    }

    [Fact]
    public void DeathlessGames_CountAsOneDeath()
    {
        var stats = StatsCalculator.Calculate("Player#EUNE", [Played(1, "Yasuo", true, 5, 0, 5)], Icons);

        Assert.Equal(10, stats.Kda);
    }

    [Fact]
    public void NoGames_IsAllZeros()
    {
        var stats = StatsCalculator.Calculate("Player#EUNE", [], Icons);

        Assert.Equal(0, stats.Games);
        Assert.Equal(0, stats.WinRate);
        Assert.Empty(stats.Champions);
    }
}
