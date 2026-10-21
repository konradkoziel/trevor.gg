using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.Home;

// GET /api/rotation: this week's free champions, for the home page
public class RotationFunction(IRiotApiClient riot, FreeRotation rotation)
{
    [Function("Rotation")]
    public Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rotation")] HttpRequest req,
        CancellationToken ct) =>
        RiotCalls.RunAsync(riot, async () => ApiResults.StaticData(req, await rotation.ChampionsAsync(ct)));
}
