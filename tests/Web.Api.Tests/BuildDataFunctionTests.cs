using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Builds;

namespace Web.Api.Tests;

public class BuildDataFunctionTests
{
    [Fact]
    public async Task ReturnsItemsRunesAndSpells_Cacheable()
    {
        var catalog = new FakeCatalog();
        var context = new DefaultHttpContext();

        var result = await new BuildDataFunction(catalog).Run(context.Request, CancellationToken.None);

        var data = Assert.IsType<BuildData>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Same(catalog.Items, data.Items);
        Assert.Same(catalog.RuneTrees, data.RuneTrees);
        Assert.Same(catalog.SummonerSpells, data.SummonerSpells);
        Assert.Equal("public, max-age=3600", context.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task AnyPartMissing_Is503()
    {
        var result = await new BuildDataFunction(new FakeCatalog { RuneTrees = null }).Run(new DefaultHttpContext().Request, CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }
}
