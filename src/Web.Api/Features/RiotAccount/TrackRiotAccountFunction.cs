using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// PUT /api/me/riot/{accountId}/tracked switches the account's match reports on Discord on, DELETE switches them off.
// The bot reads the switched-on accounts from GET /api/bot/tracked
public class TrackRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store)
{
    [Function("TrackRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", "delete", Route = "me/riot/{accountId}/tracked")] HttpRequest req,
        string accountId,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var accounts = await store.ListAccountsAsync(user.DiscordId, ct);
        var account = accounts.FirstOrDefault(a => a.Puuid == accountId);
        if (account is null)
            return new NotFoundResult();

        var changed = account with { Tracked = HttpMethods.IsPut(req.Method) };
        if (changed != account)
            await store.SaveLinkAsync(changed, ct);
        return new OkObjectResult(RiotAccountsView.Of([.. accounts.Where(a => a.Puuid != accountId), changed]));
    }
}
