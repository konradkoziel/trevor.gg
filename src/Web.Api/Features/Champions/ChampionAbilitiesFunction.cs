using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.Champions;

// GET /api/champions/{id}/abilities: passive and Q/W/E/R, for the skill order
public class ChampionAbilitiesFunction(IDataDragon dataDragon, IDataDragonCatalog catalog)
{
    [Function("ChampionAbilities")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "champions/{id}/abilities")] HttpRequest req,
        string id,
        CancellationToken ct)
    {
        // Only ids from the champion list reach Data Dragon, nothing typed into the URL
        var champions = await dataDragon.ChampionsAsync(ct);
        if (champions is null)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        if (!champions.Any(c => c.Id == id))
            return new NotFoundResult();

        return ApiResults.StaticData(req, await catalog.ChampionAbilitiesAsync(id, ct));
    }
}
