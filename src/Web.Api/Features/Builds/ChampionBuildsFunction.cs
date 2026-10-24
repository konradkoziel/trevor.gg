using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// GET /api/champions/{championId}/builds: what the team built on one champion, newest first; their names, so logged-in only
public class ChampionBuildsFunction(ISessionTokens sessions, IBuildStore store)
{
    [Function("ChampionBuilds")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "champions/{championId}/builds")] HttpRequest req,
        string championId,
        CancellationToken ct)
    {
        if (await sessions.ReadAsync(req, ct) is null)
            return new UnauthorizedResult();

        var builds = await store.ListByChampionAsync(championId, ct);
        return new OkObjectResult(builds.Select(RecentBuild.From).ToList());
    }
}
