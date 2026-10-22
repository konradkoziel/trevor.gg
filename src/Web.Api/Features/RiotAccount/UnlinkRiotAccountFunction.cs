using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// DELETE /api/me/riot/{accountId}: unlink one account; when it was the main one, the oldest of the rest takes over
public class UnlinkRiotAccountFunction(ISessionTokens sessions, IRiotLinkStore store)
{
    [Function("UnlinkRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "me/riot/{accountId}")] HttpRequest req,
        string accountId,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var accounts = await store.ListAccountsAsync(user.DiscordId, ct);
        var removed = accounts.FirstOrDefault(a => a.Puuid == accountId);
        if (removed is null)
            return new NotFoundResult();

        await store.RemoveAsync(user.DiscordId, accountId, ct);
        var rest = accounts.Where(a => a != removed).ToList();
        if (removed == RiotLinks.MainOf(accounts) && RiotLinks.MainOf(rest) is { } next)
        {
            await store.SaveLinkAsync(next with { IsMain = true }, ct);
            rest = rest.Select(a => a == next ? next with { IsMain = true } : a).ToList();
        }
        return new OkObjectResult(RiotAccountsView.Of(rest));
    }
}
