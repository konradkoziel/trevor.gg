using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using trevor.Infrastructure.Web;

namespace trevor.Tests;

public class WebApiClientTests
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

    private static WebApiClient Client(StubHandler handler, string? url = "https://api.test/") =>
        new(new HttpClient(handler), Options.Create(new WebApiOptions { Url = url, Key = "k-1", SiteUrl = "https://site.test" }),
            NullLogger<WebApiClient>.Instance);

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task LinkedPlayer_SendsTheKey_AndReadsTheAnswer()
    {
        var handler = new StubHandler(_ => Json("""{"riotId":"Player#EUNE","puuid":"p1"}"""));

        var player = await Client(handler).GetLinkedPlayerAsync("user-1");

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.test/api/bot/players/user-1", request.RequestUri!.AbsoluteUri);
        Assert.Equal("k-1", request.Headers.GetValues(WebApiClient.KeyHeader).Single());
        Assert.Equal(new LinkedPlayer("Player#EUNE", "p1"), player);
    }

    [Fact]
    public async Task TrackedPlayers_ReadsTheList()
    {
        var handler = new StubHandler(_ => Json("""[{"riotId":"Player#EUNE","puuid":"p1"}]"""));

        var tracked = await Client(handler).GetTrackedPlayersAsync();

        Assert.Equal("https://api.test/api/bot/tracked", handler.Requests[0].RequestUri!.AbsoluteUri);
        Assert.Equal([new LinkedPlayer("Player#EUNE", "p1")], tracked!);
    }

    [Theory]
    [InlineData(true, "PUT")]
    [InlineData(false, "DELETE")]
    public async Task Tracking_SwitchesByRiotId(bool tracked, string method)
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        Assert.Equal(WebTracking.Switched, await Client(handler).SetTrackedAsync("Nick Two#EUNE", tracked));
        Assert.Equal(method, handler.Requests[0].Method.Method);
        Assert.Equal("https://api.test/api/bot/tracked/Nick%20Two/EUNE", handler.Requests[0].RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task Tracking_TellsUnlinkedFromUnavailable()
    {
        Assert.Equal(WebTracking.NotLinked,
            await Client(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound))).SetTrackedAsync("Nick#EUNE", true));
        Assert.Equal(WebTracking.Unavailable,
            await Client(new StubHandler(_ => throw new HttpRequestException("down"))).SetTrackedAsync("Nick#EUNE", true));
        Assert.Equal(WebTracking.Unavailable,
            await Client(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)), url: null).SetTrackedAsync("Nick#EUNE", true));
    }

    [Fact]
    public async Task Builds_EscapeTheChampion()
    {
        var handler = new StubHandler(_ => Json("[]"));

        Assert.Empty((await Client(handler).GetBuildsAsync("user-1", "Kai'Sa & co"))!);
        Assert.Equal("https://api.test/api/bot/players/user-1/builds?champion=Kai%27Sa%20%26%20co", handler.Requests[0].RequestUri!.AbsoluteUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task FailedOrMissing_IsNull(HttpStatusCode status) =>
        Assert.Null(await Client(new StubHandler(_ => new HttpResponseMessage(status))).GetLinkedPlayerAsync("user-1"));

    [Fact]
    public async Task SiteDown_IsNull() =>
        Assert.Null(await Client(new StubHandler(_ => throw new HttpRequestException("down"))).GetLinkedPlayerAsync("user-1"));

    [Fact]
    public async Task NotConfigured_DoesNotCallAnything()
    {
        var handler = new StubHandler(_ => Json("{}"));

        Assert.Null(await Client(handler, url: null).GetLinkedPlayerAsync("user-1"));
        Assert.Empty(handler.Requests);
    }
}
