using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Builds;
using Web.Api.Features.Share;

namespace Web.Api.Tests;

public class ShareBuildFunctionTests
{
    private const string Id = "AbCdEfGhIj_";
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly Champion[] Champions =
        [new("Yasuo", "Yasuo", "Pokutnik", "https://icons.test/Yasuo.png", Key: 157, SplashUrl: "https://splash.test/Yasuo_0.jpg")];

    private readonly FakeBuildStore _builds = new();

    private async Task<(IActionResult Result, HttpResponse Response)> Run(string id, FakeCatalog? catalog = null)
    {
        var context = new DefaultHttpContext();
        var options = Microsoft.Extensions.Options.Options.Create(new ShareOptions { SiteUrl = "https://site.test/" });
        var result = await new ShareBuildFunction(options, _builds, new FakeDataDragon(Champions), catalog ?? TestBuilds.Catalog())
            .Run(context.Request, id, CancellationToken.None);
        return (result, context.Response);
    }

    private void SaveBuild(string name = "Yasuo, który boli") =>
        _builds.Builds[Id] = new StoredBuild(Id, "1", "Ann", Now, TestBuilds.Valid() with { Name = name });

    [Fact]
    public async Task SavedBuild_GetsOpenGraphTags_AndSendsPeopleToTheSite()
    {
        SaveBuild();

        var (result, response) = await Run(Id);

        var html = Assert.IsType<ContentResult>(result).Content!;
        Assert.Contains("""<meta property="og:title" content="Yasuo, który boli · Yasuo">""", html);
        Assert.Contains("Build od Ann.", html);
        Assert.Contains("""<meta property="og:image" content="https://splash.test/Yasuo_0.jpg">""", html);
        Assert.Contains("""<meta http-equiv="refresh" content="0; url=https://site.test/b/AbCdEfGhIj_">""", html);
        Assert.Equal("public, max-age=300", response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task BuildName_IsHtmlEncoded()
    {
        SaveBuild("""<script>alert("x")</script>""");

        var html = Assert.IsType<ContentResult>((await Run(Id)).Result).Content!;

        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public async Task DataDragonDown_StillPreviewsTheName()
    {
        SaveBuild();

        var html = Assert.IsType<ContentResult>((await Run(Id, new FakeCatalog { Items = null })).Result).Content!;

        Assert.Contains(BuildPreviews.FallbackDescription, html);
        Assert.DoesNotContain("og:image", html);
    }

    [Theory]
    [InlineData(Id)]
    [InlineData("not-an-id")]
    public async Task UnknownBuild_RedirectsToTheSite(string id)
    {
        var redirect = Assert.IsType<RedirectResult>((await Run(id)).Result);

        Assert.StartsWith("https://site.test/b/", redirect.Url);
    }
}
