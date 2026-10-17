using trevor.Features.Lol.Common;
using trevor.Features.Lol.Roast;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

// Player lookup and Riot errors are covered by RiotPlayerRunnerTests
public class RoastCommandTests
{
    private static RiotMatchParticipant Player(
        string puuid, int teamId, bool win, int k, int d, int a, int damage,
        string champion = "Teemo", string? position = "TOP", int minions = 150, int neutral = 10, int vision = 12) =>
        new(puuid, champion, teamId, position, win, k, d, a, minions, neutral, damage, vision, 9000);

    // Our player: Yasuo mid, 2/9/3, 12 400 dmg, 30 min ranked solo loss
    private static RiotMatch Match(long duration = 1800) => new(new RiotMatchInfo(duration, 420, "CLASSIC",
    [
        Player("puuid-1", 100, false, 2, 9, 3, 12_400, "Yasuo", "MIDDLE", minions: 120, neutral: 6, vision: 7),
        Player("ally-1", 100, false, 5, 4, 6, 20_000),
        Player("ally-2", 100, false, 3, 5, 8, 15_000),
        Player("ally-3", 100, false, 1, 6, 10, 9_000),
        Player("ally-4", 100, false, 0, 3, 12, 3_600),
        Player("enemy-1", 200, true, 15, 2, 4, 30_000),
    ]));

    private static Task<string> Run(FakeRiot riot, FakeAi? ai = null) =>
        new RoastCommand(new RiotPlayerRunner(riot), riot, ai ?? new FakeAi("Dziewięć śmierci na Yasuo. Klasyk.")).ExecuteAsync(
            TestInteractions.Command("roast", """[{ "name": "nick", "type": 3, "value": "Player#EUNE" }]"""));

    [Fact]
    public void Stats_AreComputedFromTheTeam()
    {
        var s = MatchStats.For(Match(), "puuid-1")!;

        Assert.Equal("Yasuo", s.Champion);
        Assert.Equal("Mid", s.Role);
        Assert.Equal("Ranked Solo/Duo", s.Queue);
        Assert.Equal(126, s.Cs);
        Assert.Equal(4.2, s.CsPerMinute, 3);
        Assert.Equal(12_400 / 60_000.0, s.DamageShare, 3);
        Assert.Equal(3, s.DamageRankInTeam);
        Assert.Equal(5 / 11.0, s.KillParticipation, 3);
    }

    [Fact]
    public void Stats_AreNull_WhenPlayerIsNotInMatch()
    {
        Assert.Null(MatchStats.For(Match(), "someone-else"));
    }

    [Fact]
    public async Task Roast_ShowsStatsHeader_AndAiText()
    {
        var text = await Run(new FakeRiot { Match = Match() });

        Assert.StartsWith("🔥 **Player#EUNE** — Yasuo (Mid), Ranked Solo/Duo, **Przegrana** w 30 min\n", text);
        Assert.Contains("KDA **2/9/3** · CS 126 (4.2/min) · DMG 12.4k (21% drużyny) · KP 45% · Wizja 7", text);
        Assert.EndsWith("Dziewięć śmierci na Yasuo. Klasyk.", text);
    }

    [Fact]
    public async Task AiGetsStats_AndRoastInstruction()
    {
        var ai = new FakeAi();
        await Run(new FakeRiot { Match = Match() }, ai);

        Assert.Equal(TrevorPrompts.MatchRoast, ai.LastInstruction);
        Assert.Contains("KDA: 2/9/3", ai.LastInput);
        Assert.Contains("3. miejsce w drużynie", ai.LastInput);
        Assert.Contains("Wynik: przegrana", ai.LastInput);
    }

    [Fact]
    public async Task AiUnavailable_FallsBackToCannedRoast()
    {
        var text = await Run(new FakeRiot { Match = Match() }, new FakeAi(reply: null));

        Assert.EndsWith("*9 śmierci. Wrogowie powinni ci płacić za darmowe złoto.*", text);
    }

    [Fact]
    public async Task Remake_IsNotRoastedByAi()
    {
        var ai = new FakeAi();

        var text = await Run(new FakeRiot { Match = Match(duration: 200) }, ai);

        Assert.Contains("Remake", text);
        Assert.Equal(0, ai.Calls);
    }

    [Fact]
    public async Task NoMatches_SaysSo()
    {
        Assert.Contains("nie ma żadnego meczu", await Run(new FakeRiot { MatchIds = [] }));
    }
}
