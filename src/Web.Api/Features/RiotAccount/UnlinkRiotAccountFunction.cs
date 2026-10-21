using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// DELETE /api/me/riot
public class UnlinkRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store)
{
    [Function("UnlinkRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "me/riot")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        await store.RemoveAsync(user.DiscordId, ct);
        return new NoContentResult();
    }
}
