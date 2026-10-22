using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Web.Api.Features.Bot;
using Web.Api.Features.Builds;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Tests;

public class BotFunctionsTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly IOptions<BotApiOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new BotApiOptions { Key = "k-1", SiteUrl = "https://site.test/" });

    private readonly FakeRiotLinkStore _links = new();
    private readonly FakeBuildStore _builds = new();

    private static HttpRequest Request(string? key = "k-1", string? champion = null)
    {
        var context = new DefaultHttpContext();
        if (key is not null)
            context.Request.Headers[BotAuth.KeyHeader] = key;
        if (champion is not null)
            context.Request.QueryString = QueryString.Create("champion", champion);
        return context.Request;
    }

    private Task<IActionResult> Builds(HttpRequest req, IOptions<BotApiOptions>? options = null) =>
        new BotBuildsFunction(options ?? Options, _builds, new FakeDataDragon(TestBuilds.Champions), TestBuilds.Catalog())
            .Run(req, "1", CancellationToken.None);

    [Theory]
    [InlineData(null)]
    [InlineData("wrong")]
    public async Task WithoutTheRightKey_Is401(string? key)
    {
        _links.Add(new RiotLink("1", "p", "Player#EUNE", Now));

        Assert.IsType<UnauthorizedResult>(await new BotPlayerFunction(Options, _links).Run(Request(key), "1", CancellationToken.None));
    }

    [Fact]
    public async Task WithoutAKeyConfigured_Is503()
    {
        var result = await new BotPlayerFunction(Microsoft.Extensions.Options.Options.Create(new BotApiOptions()), _links)
            .Run(Request(), "1", CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Player_IsTheLinkedAccount_Or404()
    {
        _links.Add(new RiotLink("1", "p", "Player#EUNE", Now));
        var function = new BotPlayerFunction(Options, _links);

        Assert.Equal(new LinkedPlayer("Player#EUNE", "p"),
            Assert.IsType<OkObjectResult>(await function.Run(Request(), "1", CancellationToken.None)).Value);
        Assert.IsType<NotFoundResult>(await function.Run(Request(), "2", CancellationToken.None));
    }

    [Fact]
    public async Task Tracked_AreTheSwitchedOnAccountsOfEveryone()
    {
        _links.Add(new RiotLink("1", "a", "Ann#EUNE", Now, Tracked: true));
        _links.Add(new RiotLink("1", "s", "Smurf#EUNE", Now));
        _links.Add(new RiotLink("2", "b", "Bob#EUNE", Now, Tracked: true));

        var result = await new BotTrackedPlayersFunction(Options, _links).Run(Request(), CancellationToken.None);

        Assert.Equal([new LinkedPlayer("Ann#EUNE", "a"), new LinkedPlayer("Bob#EUNE", "b")],
            Assert.IsAssignableFrom<IReadOnlyList<LinkedPlayer>>(Assert.IsType<OkObjectResult>(result).Value));
    }

    [Fact]
    public async Task Tracked_NeedsTheKey() =>
        Assert.IsType<UnauthorizedResult>(await new BotTrackedPlayersFunction(Options, _links).Run(Request("wrong"), CancellationToken.None));

    private Task<IActionResult> SetTracking(string method, string name, string tag, string? key = "k-1")
    {
        var req = Request(key);
        req.Method = method;
        return new BotTrackingFunction(Options, _links).Run(req, name, tag, CancellationToken.None);
    }

    [Fact]
    public async Task Tracking_SwitchesTheLinkedAccount_IgnoringCase()
    {
        _links.Add(new RiotLink("1", "a", "Ann#EUNE", Now, IsMain: true));

        Assert.Equal(new LinkedPlayer("Ann#EUNE", "a"), Assert.IsType<OkObjectResult>(await SetTracking("PUT", "ann", "eune")).Value);
        Assert.True(_links.Accounts.Single().Tracked);

        await SetTracking("DELETE", "Ann", "EUNE");
        Assert.False(_links.Accounts.Single().Tracked);
        Assert.True(_links.Accounts.Single().IsMain);
    }

    [Fact]
    public async Task Tracking_UnlinkedAccount_Is404_AndNeedsTheKey()
    {
        _links.Add(new RiotLink("1", "a", "Ann#EUNE", Now));

        Assert.IsType<NotFoundResult>(await SetTracking("PUT", "Bob", "EUNE"));
        Assert.IsType<UnauthorizedResult>(await SetTracking("PUT", "Ann", "EUNE", key: "wrong"));
        Assert.False(_links.Accounts.Single().Tracked);
    }

    [Fact]
    public async Task Builds_AreCardsWithNames_NewestFirst_ForThatChampionOnly()
    {
        _builds.Builds["AAAAAAAAAAA"] = new StoredBuild("AAAAAAAAAAA", "1", "Ann", Now, TestBuilds.Valid());
        _builds.Builds["BBBBBBBBBBB"] = new StoredBuild("BBBBBBBBBBB", "1", "Ann", Now.AddDays(1), TestBuilds.Valid() with { Name = "Nowszy" });
        _builds.Builds["CCCCCCCCCCC"] = new StoredBuild("CCCCCCCCCCC", "2", "Bob", Now.AddDays(2), TestBuilds.Valid());

        var cards = Assert.IsAssignableFrom<IReadOnlyList<BuildCard>>(Assert.IsType<OkObjectResult>(await Builds(Request(champion: "yasuo"))).Value);

        Assert.Equal(["Nowszy", "Yasuo, który boli"], cards.Select(c => c.Name));
        var card = cards[1];
        Assert.Equal("https://site.test/b/AAAAAAAAAAA", card.Url);
        Assert.Equal(["Ostrze Dorana", "Nagolenniki Berserkera", "Nieskończona Krawędź"], card.Items);
        Assert.Equal("Rune 8005", card.Keystone);
        Assert.Equal(["Rune 9101", "Rune 9104", "Rune 8014", "Rune 8446", "Rune 8451"], card.Runes);
        Assert.Equal(["Precyzja", "Determinacja"], card.RuneTrees);
        Assert.Equal(["Błysk", "Podpalenie"], card.SummonerSpells);
        Assert.Equal(["Q", "E", "W"], card.SkillMaxOrder);
    }

    [Fact]
    public async Task Builds_UnknownChampion_Is404() =>
        Assert.IsType<NotFoundResult>(await Builds(Request(champion: "Nobody")));

    [Fact]
    public async Task Builds_NeedTheKey() => Assert.IsType<UnauthorizedResult>(await Builds(Request(key: "x", champion: "Yasuo")));

    [Theory]
    [InlineData("yasuo")]
    [InlineData(" YASUO ")]
    [InlineData("Yasuo")]
    public void ChampionNames_IgnoreCaseAndSpaces(string typed) =>
        Assert.Equal("Yasuo", ChampionNames.Find(TestBuilds.Champions, typed)?.Id);

    [Fact]
    public void ChampionNames_MatchNameOrIdWithoutPunctuationAndAccents()
    {
        Lol.Shared.Riot.Models.Champion[] champions = [new("MonkeyKing", "Wukong", "", ""), new("Kaisa", "Kai'Sa", "", ""), new("Nunu", "Nunu i Willump", "", "")];

        Assert.Equal("MonkeyKing", ChampionNames.Find(champions, "wukong")?.Id);
        Assert.Equal("MonkeyKing", ChampionNames.Find(champions, "monkeyking")?.Id);
        Assert.Equal("Kaisa", ChampionNames.Find(champions, "kai'sa")?.Id);
        Assert.Equal("Nunu", ChampionNames.Find(champions, "Nunu i Willump")?.Id);
        Assert.Null(ChampionNames.Find(champions, ""));
    }

    [Fact]
    public void MaxOrder_IsTheOrderBasicsReachRankFive() =>
        Assert.Equal(["Q", "E", "W"], SkillOrderRules.MaxOrder(TestBuilds.QMax));
}
