using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Champions;

namespace Web.Api.Tests;

public class ChampionAbilitiesFunctionTests
{
    private static readonly Champion[] Champions = [new("Yasuo", "Yasuo", "Pokutnik", "https://icons.test/Yasuo.png")];

    private static Task<IActionResult> Run(string id, FakeCatalog catalog, IReadOnlyList<Champion>? champions = null) =>
        new ChampionAbilitiesFunction(new FakeDataDragon(champions ?? Champions), catalog)
            .Run(new DefaultHttpContext().Request, id, CancellationToken.None);

    [Fact]
    public async Task KnownChampion_ReturnsAbilities()
    {
        var catalog = new FakeCatalog();

        var result = await Run("Yasuo", catalog);

        Assert.Same(catalog.Abilities, Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal("Yasuo", catalog.RequestedChampion);
    }

    [Theory]
    [InlineData("Nobody")]
    [InlineData("..%2F..%2Fsecret")]
    public async Task UnknownChampion_Is404_WithoutAskingDataDragon(string id)
    {
        var catalog = new FakeCatalog();

        Assert.IsType<NotFoundResult>(await Run(id, catalog));
        Assert.Null(catalog.RequestedChampion);
    }

    [Fact]
    public async Task DataDragonDown_Is503()
    {
        var result = await new ChampionAbilitiesFunction(new FakeDataDragon(null), new FakeCatalog())
            .Run(new DefaultHttpContext().Request, "Yasuo", CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }
}
