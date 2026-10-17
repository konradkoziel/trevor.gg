using trevor.Features.Lol.Common;
using trevor.Features.Lol.Roast;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

// Player lookup and Riot errors are covered by RiotPlayerRunnerTests
public class RoastCommandTests
{
    private const int LossColor = 0xE74C3C;
    private const int WinColor = 0x2ECC71;
    private const int RemakeColor = 0x95A5A6;

    private static RiotMatchParticipant Player(
        string puuid, int teamId, bool win, int k, int d, int a, int damage,
        string champion = "Teemo", string? position = "TOP", int minions = 150, int neutral = 10, int vision = 12) =>
        new(puuid, champion, teamId, position, win, k, d, a, minions, neutral, damage, vision, 9000);

    // Our player: Yasuo mid, 2/9/3, 12 400 dmg, 30 min ranked solo
    private static RiotMatch Match(long duration = 1800, bool win = false) => new(new RiotMatchInfo(duration, 420, "CLASSIC",
    [
        Player("puuid-1", 100, win, 2, 9, 3, 12_400, "Yasuo", "MIDDLE", minions: 120, neutral: 6, vision: 7),
        Player("ally-1", 100, win, 5, 4, 6, 20_000),
        Player("ally-2", 100, win, 3, 5, 8, 15_000),
        Player("ally-3", 100, win, 1, 6, 10, 9_000),
        Player("ally-4", 100, win, 0, 3, 12, 3_600),
        Player("enemy-1", 200, !win, 15, 2, 4, 30_000),
    ]));

    private static Task<DiscordResponseData> Run(FakeRiot riot, FakeAi? ai = null, FakeDataDragon? dataDragon = null) =>
        new RoastCommand(
                new RiotPlayerRunner(riot), riot, dataDragon ?? new FakeDataDragon(),
                ai ?? new FakeAi("Dziewięć śmierci na Yasuo. Klasyk."))
            .ExecuteAsync(TestInteractions.Command("roast", """[{ "name": "nick", "type": 3, "value": "Player#EUNE" }]"""));

    private static DiscordEmbed SingleEmbed(DiscordResponseData message) => Assert.Single(message.Embeds!);

    private static string Field(DiscordEmbed embed, string name) =>
        Assert.Single(embed.Fields!, f => f.Name == name).Value;

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
    public async Task Roast_IsAnEmbed_WithStatsAndAiVerdict()
    {
        var message = await Run(new FakeRiot { Match = Match() });

        Assert.Null(message.Content);
        var embed = SingleEmbed(message);
        Assert.Equal("🔥 Player#EUNE — Yasuo (Mid)", embed.Title);
        Assert.Equal("Ranked Solo/Duo · **Przegrana** · 30 min", embed.Description);
        Assert.Equal(LossColor, embed.Color);
        Assert.Equal("https://icons.test/Yasuo.png", embed.Thumbnail?.Url);
        Assert.Equal("2/9/3", Field(embed, "KDA"));
        Assert.Equal("126 (4.2/min)", Field(embed, "CS"));
        Assert.Equal("12.4k (21%)", Field(embed, "Obrażenia"));
        Assert.Equal("45%", Field(embed, "Udział w zabójstwach"));
        Assert.Equal("7", Field(embed, "Wizja"));
        Assert.Equal("Dziewięć śmierci na Yasuo. Klasyk.", Field(embed, "Werdykt Trevora"));
        Assert.False(embed.Fields!.Last().Inline);
    }

    [Fact]
    public async Task Win_IsGreen()
    {
        var embed = SingleEmbed(await Run(new FakeRiot { Match = Match(win: true) }));

        Assert.Equal(WinColor, embed.Color);
        Assert.Contains("**Wygrana**", embed.Description);
    }

    [Fact]
    public async Task MissingIcon_LeavesThumbnailOut()
    {
        var embed = SingleEmbed(await Run(new FakeRiot { Match = Match() }, dataDragon: new FakeDataDragon(iconUrl: null)));

        Assert.Null(embed.Thumbnail);
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
        var embed = SingleEmbed(await Run(new FakeRiot { Match = Match() }, new FakeAi(reply: null)));

        Assert.Equal("*9 śmierci. Wrogowie powinni ci płacić za darmowe złoto.*", Field(embed, "Werdykt Trevora"));
    }

    [Fact]
    public async Task Remake_IsGrey_AndNotRoastedByAi()
    {
        var ai = new FakeAi();

        var embed = SingleEmbed(await Run(new FakeRiot { Match = Match(duration: 200) }, ai));

        Assert.Equal(RemakeColor, embed.Color);
        Assert.Contains("Remake", Field(embed, "Werdykt Trevora"));
        Assert.Equal(0, ai.Calls);
    }

    [Fact]
    public async Task NoMatches_IsPlainText()
    {
        var message = await Run(new FakeRiot { MatchIds = [] });

        Assert.Null(message.Embeds);
        Assert.Contains("nie ma żadnego meczu", message.Content);
    }
}
