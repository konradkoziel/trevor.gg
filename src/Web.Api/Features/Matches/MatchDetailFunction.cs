using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Matches;

// GET /api/matches/{matchId}: everyone's stats and builds in one game; friends' games, so logged-in users only
public class MatchDetailFunction(ISessionTokens sessions, IRiotApiClient riot, MatchDetailSource source)
{
    [Function("MatchDetail")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "matches/{matchId}")] HttpRequest req,
        string matchId,
        CancellationToken ct)
    {
        if (await sessions.ReadAsync(req, ct) is null)
            return new UnauthorizedResult();
        if (!MatchIds.IsWellFormed(matchId))
            return new NotFoundResult();

        return await RiotCalls.RunAsync(riot, async () =>
            await source.GetAsync(matchId, ct) is { } detail ? new OkObjectResult(detail) : new NotFoundResult());
    }
}
