using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Lol.Shared.Riot;

namespace Lol.Shared.Tests;

public class DataDragonClientTests
{
    private sealed class VersionsHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++;
            Assert.Equal("https://ddragon.leagueoflegends.com/api/versions.json", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(respond());
        }
    }

    private static HttpResponseMessage Versions(params string[] versions) => new(HttpStatusCode.OK)
    {
        Content = new StringContent($"[{string.Join(",", versions.Select(v => $"\"{v}\""))}]", Encoding.UTF8, "application/json")
    };

    private static DataDragonClient Client(VersionsHandler handler, FakeTimeProvider? time = null) =>
        new(new DataDragonCdn(new HttpClient(handler), time ?? new FakeTimeProvider(), NullLogger<DataDragonCdn>.Instance));

    [Fact]
    public async Task IconUrl_UsesLatestVersion()
    {
        var url = await Client(new VersionsHandler(() => Versions("16.20.1", "16.19.1"))).ChampionIconUrlAsync("Yasuo");

        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/16.20.1/img/champion/Yasuo.png", url);
    }

    [Fact]
    public async Task ProfileIconUrl_UsesLatestVersion()
    {
        var url = await Client(new VersionsHandler(() => Versions("16.20.1"))).ProfileIconUrlAsync(7);

        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/16.20.1/img/profileicon/7.png", url);
    }

    [Fact]
    public async Task ItemIconUrl_UsesLatestVersion()
    {
        var url = await Client(new VersionsHandler(() => Versions("16.20.1"))).ItemIconUrlAsync(3340);

        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/16.20.1/img/item/3340.png", url);
    }

    [Fact]
    public async Task MatchV5Spelling_IsMappedToDataDragonId()
    {
        var url = await Client(new VersionsHandler(() => Versions("16.20.1"))).ChampionIconUrlAsync("FiddleSticks");

        Assert.EndsWith("/img/champion/Fiddlesticks.png", url);
    }

    [Fact]
    public async Task Version_IsCached_UntilItExpires()
    {
        var handler = new VersionsHandler(() => Versions("16.20.1"));
        var time = new FakeTimeProvider();
        var client = Client(handler, time);

        await client.ChampionIconUrlAsync("Yasuo");
        time.Now += TimeSpan.FromHours(5);
        await client.ChampionIconUrlAsync("Ahri");
        Assert.Equal(1, handler.Calls);

        time.Now += TimeSpan.FromHours(2);
        await client.ChampionIconUrlAsync("Ahri");
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Unreachable_ReturnsNull()
    {
        var url = await Client(new VersionsHandler(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)))
            .ChampionIconUrlAsync("Yasuo");

        Assert.Null(url);
    }

    [Fact]
    public async Task FailedRefresh_KeepsStaleVersion()
    {
        var fail = false;
        var handler = new VersionsHandler(() => fail ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : Versions("16.20.1"));
        var time = new FakeTimeProvider();
        var client = Client(handler, time);

        await client.ChampionIconUrlAsync("Yasuo");
        fail = true;
        time.Now += TimeSpan.FromHours(7);

        Assert.Contains("/16.20.1/", await client.ChampionIconUrlAsync("Yasuo"));
    }
}
