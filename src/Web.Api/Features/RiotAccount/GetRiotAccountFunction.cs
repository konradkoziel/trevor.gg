using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// GET /api/me/riot
public class GetRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store)
{
    [Function("GetRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me/riot")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        return new OkObjectResult(RiotAccountsView.Of(await store.ListAccountsAsync(user.DiscordId, ct)));
    }
}
