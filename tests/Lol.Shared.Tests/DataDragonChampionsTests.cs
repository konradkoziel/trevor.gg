using System.Net;
using System.Text;
using Lol.Shared.Riot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lol.Shared.Tests;

public class DataDragonChampionsTests
{
    private const string VersionsUrl = "https://ddragon.leagueoflegends.com/api/versions.json";

    private sealed class RoutingHandler(Dictionary<string, Func<HttpResponseMessage>> routes) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var url = request.RequestUri!.AbsoluteUri;
            Requests.Add(url);
            return Task.FromResult(routes.TryGetValue(url, out var respond) ? respond() : new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static string ChampionsUrl(string version) =>
        $"https://ddragon.leagueoflegends.com/cdn/{version}/data/pl_PL/champion.json";

    private const string ChampionList = """
        {
          "type": "champion",
          "data": {
            "Yasuo": { "id": "Yasuo", "key": "157", "name": "Yasuo", "title": "Pokutnik" },
            "MonkeyKing": { "id": "MonkeyKing", "key": "62", "name": "Wukong", "title": "Małpi Król", "blurb": "Małpa z kijem." },
            "Ahri": { "id": "Ahri", "key": "103", "name": "Ahri", "title": "Dziewięcioogoniasta Lisica" }
          }
        }
        """;

    private static DataDragonClient Client(RoutingHandler handler, FakeTimeProvider? time = null) =>
        new(new DataDragonCdn(new HttpClient(handler), time ?? new FakeTimeProvider(), NullLogger<DataDragonCdn>.Instance));

    [Fact]
    public async Task Champions_AreSortedByName_WithIconsFromTheirId()
    {
        var handler = new RoutingHandler(new()
        {
            [VersionsUrl] = () => Json("""["16.20.1"]"""),
            [ChampionsUrl("16.20.1")] = () => Json(ChampionList),
        });

        var champions = await Client(handler).ChampionsAsync();

        Assert.Equal(["Ahri", "Wukong", "Yasuo"], champions!.Select(c => c.Name));
        var wukong = champions![1];
        Assert.Equal("MonkeyKing", wukong.Id);
        Assert.Equal("Małpi Król", wukong.Title);
        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/16.20.1/img/champion/MonkeyKing.png", wukong.IconUrl);
        Assert.Equal(62, wukong.Key);
        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/img/champion/splash/MonkeyKing_0.jpg", wukong.SplashUrl);
        Assert.Equal("Małpa z kijem.", wukong.Blurb);
    }

    [Fact]
    public async Task Champions_AreCached_ForTheSamePatch()
    {
        var handler = new RoutingHandler(new()
        {
            [VersionsUrl] = () => Json("""["16.20.1"]"""),
            [ChampionsUrl("16.20.1")] = () => Json(ChampionList),
        });
        var client = Client(handler);

        await client.ChampionsAsync();
        await client.ChampionsAsync();

        Assert.Single(handler.Requests, ChampionsUrl("16.20.1"));
    }

    [Fact]
    public async Task NewPatch_RefetchesTheList()
    {
        var version = "16.20.1";
        var handler = new RoutingHandler(new()
        {
            [VersionsUrl] = () => Json($"""["{version}"]"""),
            [ChampionsUrl("16.20.1")] = () => Json(ChampionList),
            [ChampionsUrl("16.21.1")] = () => Json(ChampionList),
        });
        var time = new FakeTimeProvider();
        var client = Client(handler, time);

        await client.ChampionsAsync();
        version = "16.21.1";
        time.Now += TimeSpan.FromHours(7);
        var champions = await client.ChampionsAsync();

        Assert.Contains(ChampionsUrl("16.21.1"), handler.Requests);
        Assert.Contains("/16.21.1/", champions![0].IconUrl);
    }

    [Fact]
    public async Task ListUnreachable_ReturnsNull()
    {
        var handler = new RoutingHandler(new() { [VersionsUrl] = () => Json("""["16.20.1"]""") });

        Assert.Null(await Client(handler).ChampionsAsync());
    }

    [Fact]
    public async Task VersionsUnreachable_ReturnsNull()
    {
        Assert.Null(await Client(new RoutingHandler([])).ChampionsAsync());
    }
}
