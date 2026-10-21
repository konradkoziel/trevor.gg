using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Home;

// GET /api/leaderboard: friends' Riot IDs, so logged-in users only
public class LeaderboardFunction(ISessionTokens sessions, IRiotApiClient riot, Leaderboard leaderboard)
{
    [Function("Leaderboard")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leaderboard")] HttpRequest req,
        CancellationToken ct)
    {
        if (await sessions.ReadAsync(req, ct) is null)
            return new UnauthorizedResult();

        return await RiotCalls.RunAsync(riot, async () => new OkObjectResult(await leaderboard.EntriesAsync(ct)));
    }
}
