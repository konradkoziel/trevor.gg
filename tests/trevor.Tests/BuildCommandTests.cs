using trevor.Features.Lol.Builds;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Tests;

public class BuildCommandTests
{
    private static readonly WebBuildCard Card = new(
        "Yasuo, który boli", "Yasuo", "https://icons.test/Yasuo.png", "Ann", "https://site.test/b/AAAAAAAAAAA",
        ["Nagolenniki Berserkera", "Nieskończona Krawędź"], "Zdobywca", ["Triumf", "Legenda: Waleczność"], ["Precyzja", "Determinacja"],
        ["Błysk", "Podpalenie"], ["Q", "E", "W"]);

    private static Task<DiscordResponseData> Run(FakeWebApi web, string? champion = "yasuo") =>
        new BuildCommand(web).ExecuteAsync(TestInteractions.Command("build",
            champion is null ? "[]" : $$"""[{ "name": "champion", "type": 3, "value": "{{champion}}" }]"""));

    [Fact]
    public async Task ShowsTheNewestBuild_AsACard()
    {
        var web = new FakeWebApi { IsConfigured = true, Builds = [Card, Card with { Name = "Stary" }] };

        var embed = Assert.Single((await Run(web)).Embeds!);

        Assert.Equal(("user-1", "yasuo"), web.RequestedBuilds);
        Assert.Equal("🛠️ Yasuo, który boli", embed.Title);
        Assert.Contains("[Otwórz na stronie](https://site.test/b/AAAAAAAAAAA)", embed.Description);
        Assert.Equal("Nagolenniki Berserkera → Nieskończona Krawędź", embed.Fields!.Single(f => f.Name == BuildMessage.ItemsTitle).Value);
        Assert.Equal("**Zdobywca**\nPrecyzja / Determinacja\nTriumf\nLegenda: Waleczność", embed.Fields!.Single(f => f.Name == BuildMessage.RunesTitle).Value);
        Assert.Equal("Błysk + Podpalenie", embed.Fields!.Single(f => f.Name == BuildMessage.SpellsTitle).Value);
        Assert.Equal("Q > E > W", embed.Fields!.Single(f => f.Name == BuildMessage.SkillsTitle).Value);
        Assert.Equal("https://icons.test/Yasuo.png", embed.Thumbnail!.Url);
        Assert.Equal($"{LolMessages.EmbedFooter} · i 1 starsze na stronie", embed.Footer!.Text);
    }

    [Fact]
    public async Task EmptyParts_ShowADash()
    {
        var empty = Card with { Items = [], Keystone = null, Runes = [], RuneTrees = [], SummonerSpells = [], SkillMaxOrder = [] };

        var embed = Assert.Single((await Run(new FakeWebApi { IsConfigured = true, Builds = [empty] })).Embeds!);

        Assert.All(embed.Fields!, f => Assert.Equal("—", f.Value));
        Assert.Equal(LolMessages.EmbedFooter, embed.Footer!.Text);
    }

    [Fact]
    public async Task NoBuild_PointsToTheWebsite() =>
        Assert.Equal(BuildMessage.NoBuild("yasuo", "https://site.test"), (await Run(new FakeWebApi { IsConfigured = true })).Content);

    [Fact]
    public async Task UnknownChampion_SaysSo() =>
        Assert.Equal(BuildMessage.UnknownChampion("Nobody"),
            (await Run(new FakeWebApi { IsConfigured = true, Builds = null }, "Nobody")).Content);

    [Fact]
    public async Task WithoutTheWebsite_SaysSo() =>
        Assert.Equal(BuildMessage.SiteNotConnected, (await Run(new FakeWebApi())).Content);

    [Fact]
    public void IsDeferred_BecauseTheWebsiteMayBeColdStarting() => Assert.True(new BuildCommand(new FakeWebApi()).IsDeferred);
}
