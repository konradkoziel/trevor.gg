using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

public class RiotApiClientTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(request);
            return Task.FromResult(respond(request));
        }
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static RiotApiClient Client(StubHandler handler, string? key = "RGAPI-test", string platform = "eun1") =>
        new(new HttpClient(handler), Options.Create(new RiotOptions { ApiKey = key, Platform = platform }));

    [Fact]
    public async Task GetAccount_UsesRegionalHost_EscapesRiotId_AndSendsKey()
    {
        var handler = new StubHandler(_ => Json("""{"puuid":"p1","gameName":"Pan Kot","tagLine":"EUNE"}"""));

        var account = await Client(handler).GetAccountAsync(new RiotId("Pan Kot", "EUNE"));

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/Pan%20Kot/EUNE",
            request.RequestUri!.AbsoluteUri);
        Assert.Equal("RGAPI-test", request.Headers.GetValues("X-Riot-Token").Single());
        Assert.Equal(new RiotAccount("p1", "Pan Kot", "EUNE"), account);
    }

    [Fact]
    public async Task GetAccount_NotFound_ReturnsNull()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        Assert.Null(await Client(handler).GetAccountAsync(new RiotId("x", "y")));
    }

    [Fact]
    public async Task GetLeagueEntries_UsesPlatformHost_AndParsesEntries()
    {
        var handler = new StubHandler(_ => Json("""
            [{"leagueId":"l","queueType":"RANKED_SOLO_5x5","tier":"GOLD","rank":"II","puuid":"p1",
              "leaguePoints":47,"wins":58,"losses":61,"veteran":false,"inactive":false,"freshBlood":false,"hotStreak":true}]
            """));

        var entries = await Client(handler, platform: "euw1").GetLeagueEntriesAsync("p1");

        Assert.Equal("https://euw1.api.riotgames.com/lol/league/v4/entries/by-puuid/p1",
            Assert.Single(handler.Requests).RequestUri!.AbsoluteUri);
        Assert.Equal(new LeagueEntry("RANKED_SOLO_5x5", "GOLD", "II", 47, 58, 61), Assert.Single(entries));
    }

    [Fact]
    public async Task ErrorStatus_ThrowsRiotApiException()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<RiotApiException>(() => Client(handler).GetLeagueEntriesAsync("p1"));
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [Fact]
    public void WithoutKey_IsNotConfigured()
    {
        Assert.False(Client(new StubHandler(_ => Json("{}")), key: null).IsConfigured);
    }

    [Theory]
    [InlineData("eun1", "europe")]
    [InlineData("na1", "americas")]
    [InlineData("kr", "asia")]
    [InlineData("oc1", "asia")]
    public void AccountRegion_FollowsPlatform(string platform, string region)
    {
        Assert.Equal(region, RiotRouting.AccountRegion(platform));
    }

    [Theory]
    [InlineData("Konrad#EUNE", "Konrad", "EUNE")]
    [InlineData("Pan # Kot#123", "Pan # Kot", "123")]
    public void RiotId_SplitsOnLastHash(string text, string name, string tag)
    {
        Assert.True(RiotId.TryParse(text, out var id));
        Assert.Equal(new RiotId(name, tag), id);
    }
}
