using trevor.Features.Lol.Common;
using trevor.Features.Lol.Rank;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

// Player lookup and Riot errors are covered by RiotPlayerRunnerTests
public class RankCommandTests
{
    private static readonly LeagueEntry[] GoldAndSilver =
    [
        new(LeagueEntry.FlexQueue, "SILVER", "I", 12, 10, 10),
        new(LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61),
    ];

    private static Task<string> Run(FakeRiot riot, FakeAi? ai = null) =>
        new RankCommand(new RiotPlayerRunner(riot), riot, ai ?? new FakeAi()).ExecuteAsync(
            TestInteractions.Command("rank", """[{ "name": "nick", "type": 3, "value": "Player#EUNE" }]"""));

    [Fact]
    public async Task Unranked_IsReported_WithoutCallingAi()
    {
        var ai = new FakeAi();

        Assert.Equal(RankMessage.Unranked("Player#EUNE"), await Run(new FakeRiot(), ai));
        Assert.Equal(0, ai.Calls);
    }

    [Fact]
    public async Task SoloAndFlex_AreFormatted_WithAiVerdict()
    {
        var text = await Run(new FakeRiot { Entries = GoldAndSilver }, new FakeAi("Gold II po 119 grach. Konsekwentna przeciętność."));

        Assert.Equal("""
                     🏅 **Player#EUNE**
                     Solo/Duo: **Gold II, 47 LP** — 58W / 61L (48.7%)
                     Flex: **Silver I, 12 LP** — 10W / 10L (50.0%)
                     *Gold II po 119 grach. Konsekwentna przeciętność.*
                     """.ReplaceLineEndings("\n"), text);
    }

    [Fact]
    public async Task AiGetsPlainSummary_AndRankInstruction()
    {
        var ai = new FakeAi();
        await Run(new FakeRiot { Entries = GoldAndSilver }, ai);

        Assert.Equal(TrevorPrompts.RankVerdict, ai.LastInstruction);
        Assert.Equal("""
                     Gracz: Player#EUNE
                     Solo/Duo: Gold II, 47 LP — 58W / 61L (48.7%)
                     Flex: Silver I, 12 LP — 10W / 10L (50.0%)
                     """.ReplaceLineEndings("\n"), ai.LastInput);
    }

    [Fact]
    public async Task AiUnavailable_FallsBackToVerdictForSoloTier()
    {
        var text = await Run(new FakeRiot { Entries = GoldAndSilver }, new FakeAi(reply: null));

        Assert.EndsWith($"*{RankMessage.FallbackVerdict("GOLD")}*", text);
    }

    [Fact]
    public async Task FlexOnly_UsesFlexForVerdict()
    {
        var text = await Run(new FakeRiot { Entries = [GoldAndSilver[0]] }, new FakeAi(reply: null));

        Assert.EndsWith($"*{RankMessage.FallbackVerdict("SILVER")}*", text);
        Assert.DoesNotContain("Solo/Duo", text);
    }

    [Fact]
    public async Task ApexTier_HasNoDivision()
    {
        var text = await Run(new FakeRiot { Entries = [new LeagueEntry(LeagueEntry.SoloQueue, "CHALLENGER", "I", 1200, 300, 200)] });

        Assert.Contains("**Challenger, 1200 LP**", text);
    }
}
