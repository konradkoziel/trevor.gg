using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Bot;

// GET /api/bot/tracked: every account whose owner switched on match reports on the "Konto" page
public class BotTrackedPlayersFunction(IOptions<BotApiOptions> options, IRiotLinkStore links)
{
    [Function("BotTrackedPlayers")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bot/tracked")] HttpRequest req,
        CancellationToken ct)
    {
        if (BotAuth.Reject(req, options.Value) is { } rejection)
            return rejection;

        var tracked = (await links.ListLinksAsync(ct)).Where(l => l.Tracked).Select(l => new LinkedPlayer(l.RiotId, l.Puuid)).ToList();
        return new OkObjectResult(tracked);
    }
}
