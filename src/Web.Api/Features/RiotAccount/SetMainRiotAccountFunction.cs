using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// PUT /api/me/riot/{accountId}/main: the account /rank, /roast and "Statystyki" use by default
public class SetMainRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store)
{
    [Function("SetMainRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "me/riot/{accountId}/main")] HttpRequest req,
        string accountId,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var accounts = await store.ListAccountsAsync(user.DiscordId, ct);
        if (accounts.All(a => a.Puuid != accountId))
            return new NotFoundResult();

        var updated = accounts.Select(a => a with { IsMain = a.Puuid == accountId }).ToList();
        foreach (var account in updated.Where(a => accounts.First(o => o.Puuid == a.Puuid).IsMain != a.IsMain))
            await store.SaveLinkAsync(account, ct);
        return new OkObjectResult(RiotAccountsView.Of(updated));
    }
}
