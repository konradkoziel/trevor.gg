using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Bot;

// PUT /api/bot/tracked/{name}/{tag} switches a linked account's match reports on (/track), DELETE switches them off
// (/untrack), the same switch as on the "Konto" page. 404 when nobody linked that Riot ID
public class BotTrackingFunction(IOptions<BotApiOptions> options, IRiotLinkStore links)
{
    [Function("BotTracking")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", "delete", Route = "bot/tracked/{name}/{tag}")] HttpRequest req,
        string name,
        string tag,
        CancellationToken ct)
    {
        if (BotAuth.Reject(req, options.Value) is { } rejection)
            return rejection;

        // Riot IDs are case-insensitive
        var riotId = $"{name}#{tag}";
        var link = (await links.ListLinksAsync(ct)).FirstOrDefault(l => string.Equals(l.RiotId, riotId, StringComparison.OrdinalIgnoreCase));
        if (link is null)
            return new NotFoundResult();

        var tracked = HttpMethods.IsPut(req.Method);
        if (link.Tracked != tracked)
            await links.SaveLinkAsync(link with { Tracked = tracked }, ct);
        return new OkObjectResult(new LinkedPlayer(link.RiotId, link.Puuid));
    }
}
