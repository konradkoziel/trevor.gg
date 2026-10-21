using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// GET /api/me/riot
public class GetRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store, IDataDragon dataDragon, TimeProvider time)
{
    [Function("GetRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me/riot")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var link = await store.GetLinkAsync(user.DiscordId, ct);
        var challenge = await store.GetChallengeAsync(user.DiscordId, ct);
        var pending = challenge is not null && challenge.ExpiresAt > time.GetUtcNow()
            ? await RiotAccountViews.ChallengeAsync(challenge, dataDragon, ct)
            : null;
        return new OkObjectResult(new RiotAccountView(link?.RiotId, pending));
    }
}
