using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.Champions;

// GET /api/champions: every champion of the latest patch, for the build creator's picker
public class ChampionsFunction(IDataDragon dataDragon)
{
    [Function("Champions")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "champions")] HttpRequest req,
        CancellationToken ct) =>
        ApiResults.StaticData(req, await dataDragon.ChampionsAsync(ct));
}
