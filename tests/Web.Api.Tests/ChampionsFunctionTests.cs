using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Champions;

namespace Web.Api.Tests;

public class ChampionsFunctionTests
{
    private static readonly Champion[] Champions =
        [new("Yasuo", "Yasuo", "Pokutnik", "https://icons.test/Yasuo.png")];

    [Fact]
    public async Task ReturnsChampions_CacheableByBrowsers()
    {
        var context = new DefaultHttpContext();

        var result = await new ChampionsFunction(new FakeDataDragon(Champions)).Run(context.Request, CancellationToken.None);

        Assert.Same(Champions, Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal("public, max-age=3600", context.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task DataDragonDown_Returns503()
    {
        var result = await new ChampionsFunction(new FakeDataDragon(null)).Run(new DefaultHttpContext().Request, CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }
}
