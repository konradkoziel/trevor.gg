using trevor.Features.Lol.Common;
using trevor.Features.Lol.Rank;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;
using trevor.Infrastructure.Riot.Models;

namespace trevor.Tests;

// Player lookup and Riot errors are covered by RiotPlayerRunnerTests
public class RankCommandTests
{
    private static readonly LeagueEntry[] GoldAndSilver =
    [
        new(LeagueEntry.FlexQueue, "SILVER", "I", 12, 10, 10),
        new(LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61),
    ];

    private static Task<DiscordResponseData> Run(FakeRiot riot, FakeAi? ai = null) =>
        new RankCommand(new RiotPlayerRunner(riot), riot, ai ?? new FakeAi()).ExecuteAsync(
            TestInteractions.Command("rank", """[{ "name": "nick", "type": 3, "value": "Player#EUNE" }]"""));

    private static async Task<DiscordEmbed> RunEmbed(FakeRiot riot, FakeAi? ai = null) =>
        Assert.Single((await Run(riot, ai)).Embeds!);

    private static string Field(DiscordEmbed embed, string name) =>
        Assert.Single(embed.Fields!, f => f.Name == name).Value;

    [Fact]
    public async Task Unranked_IsReported_WithoutCallingAi()
    {
        var ai = new FakeAi();

        var embed = await RunEmbed(new FakeRiot(), ai);

        Assert.Equal("🏅 Player#EUNE", embed.Title);
        Assert.StartsWith("Unranked.", embed.Description);
        Assert.Null(embed.Fields);
        Assert.Equal(0, ai.Calls);
    }

    [Fact]
    public async Task SoloAndFlex_AreFields_WithAiVerdict()
    {
        var embed = await RunEmbed(new FakeRiot { Entries = GoldAndSilver }, new FakeAi("Gold II po 119 grach. Konsekwentna przeciętność."));

        Assert.Equal("🏅 Player#EUNE", embed.Title);
        Assert.Equal(["Solo/Duo", "Flex", LolMessages.VerdictTitle], embed.Fields!.Select(f => f.Name));
        Assert.Equal("**Gold II, 47 LP**\n58W / 61L (48.7%)", Field(embed, "Solo/Duo"));
        Assert.Equal("**Silver I, 12 LP**\n10W / 10L (50.0%)", Field(embed, "Flex"));
        Assert.Equal("*Gold II po 119 grach. Konsekwentna przeciętność.*", Field(embed, LolMessages.VerdictTitle));
        Assert.Equal(LolMessages.EmbedFooter, embed.Footer!.Text);
    }

    [Fact]
    public async Task ColorAndEmblem_FollowSoloTier()
    {
        var embed = await RunEmbed(new FakeRiot { Entries = GoldAndSilver });

        Assert.Equal(CommunityDragon.RankEmblemUrl("GOLD"), embed.Thumbnail!.Url);
        Assert.EndsWith("/emblem-gold.png", embed.Thumbnail.Url);
        Assert.NotEqual((await RunEmbed(new FakeRiot { Entries = [GoldAndSilver[0]] })).Color, embed.Color);
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
        var embed = await RunEmbed(new FakeRiot { Entries = GoldAndSilver }, new FakeAi(reply: null));

        Assert.Equal($"*{RankMessage.FallbackVerdict("GOLD")}*", Field(embed, LolMessages.VerdictTitle));
    }

    [Fact]
    public async Task FlexOnly_UsesFlexForVerdictAndEmblem()
    {
        var embed = await RunEmbed(new FakeRiot { Entries = [GoldAndSilver[0]] }, new FakeAi(reply: null));

        Assert.Equal($"*{RankMessage.FallbackVerdict("SILVER")}*", Field(embed, LolMessages.VerdictTitle));
        Assert.Equal(CommunityDragon.RankEmblemUrl("SILVER"), embed.Thumbnail!.Url);
        Assert.DoesNotContain(embed.Fields!, f => f.Name == "Solo/Duo");
    }

    [Fact]
    public async Task ApexTier_HasNoDivision()
    {
        var embed = await RunEmbed(new FakeRiot { Entries = [new LeagueEntry(LeagueEntry.SoloQueue, "CHALLENGER", "I", 1200, 300, 200)] });

        Assert.StartsWith("**Challenger, 1200 LP**", Field(embed, "Solo/Duo"));
    }
}
