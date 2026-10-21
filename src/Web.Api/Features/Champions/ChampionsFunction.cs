using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Web.Api.Features.Champions;

// GET /api/champions: every champion of the latest patch, for the build creator's picker
public class ChampionsFunction(IDataDragon dataDragon)
{
    // The list changes once per patch, so browsers may keep it for an hour
    private const string CacheControl = "public, max-age=3600";

    [Function("Champions")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "champions")] HttpRequest req,
        CancellationToken ct)
    {
        var champions = await dataDragon.ChampionsAsync(ct);
        if (champions is null)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        req.HttpContext.Response.Headers.CacheControl = CacheControl;
        return new OkObjectResult(champions);
    }
}
