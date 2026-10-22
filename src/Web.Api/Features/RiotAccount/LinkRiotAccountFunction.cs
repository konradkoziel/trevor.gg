using System.Text.Json;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// POST /api/me/riot: link one more League account by Riot ID (the first one becomes the main one). No proof of ownership:
// only members of Trevor's server log in, and an account already linked by someone else is refused
public class LinkRiotAccountFunction(ISessionTokens sessions, IRiotApiClient riot, IRiotLinkStore store, TimeProvider time)
{
    [Function("LinkRiotAccount")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "me/riot")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var body = await ReadAsync(req, ct);
        if (!RiotId.TryParse(body?.RiotId, out var riotId))
            return new BadRequestObjectResult(new ApiError(RiotAccountMessages.BadRiotId));

        return await RiotCalls.RunAsync(riot, async () =>
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            if (account is null)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.NoSuchAccount));
            if (await riot.GetSummonerAsync(account.Puuid, ct) is null)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.NeverPlayed));
            if (await IsLinkedByAnotherAsync(account.Puuid, user.DiscordId, ct))
                return new ConflictObjectResult(new ApiError(RiotAccountMessages.AlreadyLinked));

            var mine = await store.ListAccountsAsync(user.DiscordId, ct);
            var existing = mine.FirstOrDefault(a => a.Puuid == account.Puuid);
            if (existing is null && mine.Count >= RiotLinks.MaxAccounts)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.TooManyAccounts));

            // Linking the same account again only refreshes its Riot ID (after a name change)
            var name = RiotNames.RiotId(account) ?? riotId.ToString();
            var link = existing is null
                ? new RiotLink(user.DiscordId, account.Puuid, name, time.GetUtcNow(), IsMain: mine.Count == 0)
                : existing with { RiotId = name };
            await store.SaveLinkAsync(link, ct);
            return new OkObjectResult(RiotAccountsView.Of([.. mine.Where(a => a.Puuid != link.Puuid), link]));
        });
    }

    private async Task<bool> IsLinkedByAnotherAsync(string puuid, string discordId, CancellationToken ct) =>
        (await store.ListLinksAsync(ct)).Any(l => l.Puuid == puuid && l.DiscordId != discordId);

    private static async Task<LinkRiotAccountRequest?> ReadAsync(HttpRequest req, CancellationToken ct)
    {
        try
        {
            return await req.ReadFromJsonAsync<LinkRiotAccountRequest>(ct);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return null;
        }
    }
}
