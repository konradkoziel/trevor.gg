using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.Builds;

// GET /api/build-data: items, rune trees and summoner spells of the latest patch
public class BuildDataFunction(IDataDragonCatalog catalog)
{
    [Function("BuildData")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "build-data")] HttpRequest req,
        CancellationToken ct) =>
        ApiResults.StaticData(req, await BuildDataSource.LoadAsync(catalog, ct));
}
