using System.Text.Json;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// POST /api/me/riot: link a League account by Riot ID. No proof of ownership: only members of Trevor's server log in,
// and an account already linked by someone else is refused, so nobody quietly takes a friend's stats
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

            var name = RiotNames.RiotId(account) ?? riotId.ToString();
            await store.SaveLinkAsync(new RiotLink(user.DiscordId, account.Puuid, name, time.GetUtcNow()), ct);
            return new OkObjectResult(new RiotAccountView(name));
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
