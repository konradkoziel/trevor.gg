using System.Net;
using System.Text;
using Lol.Shared.Riot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lol.Shared.Tests;

// Serves Data Dragon files by path under cdn/16.20.1/; anything else is a 404
public sealed class StubDataDragon : HttpMessageHandler
{
    public const string Version = "16.20.1";

    private readonly Dictionary<string, string> _files = new();

    public List<string> Requests { get; } = [];

    public StubDataDragon With(string path, string json)
    {
        _files[$"{DataDragonCdn.BaseUrl}/cdn/{Version}/{path}"] = json;
        return this;
    }

    public DataDragonCdn Cdn() => new(new HttpClient(this), new FakeTimeProvider(), NullLogger<DataDragonCdn>.Instance);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var url = request.RequestUri!.AbsoluteUri;
        Requests.Add(url);
        if (url == $"{DataDragonCdn.BaseUrl}/api/versions.json")
            return Task.FromResult(Json($"[\"{Version}\"]"));
        return Task.FromResult(_files.TryGetValue(url, out var json) ? Json(json) : new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}
