using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Bot;

// GET /api/bot/players/{discordId}: the user's main account, so /rank and /roast work without a nick
public class BotPlayerFunction(IOptions<BotApiOptions> options, IRiotLinkStore links)
{
    [Function("BotPlayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bot/players/{discordId}")] HttpRequest req,
        string discordId,
        CancellationToken ct)
    {
        if (BotAuth.Reject(req, options.Value) is { } rejection)
            return rejection;

        var link = await links.GetMainAsync(discordId, ct);
        return link is null ? new NotFoundResult() : new OkObjectResult(new LinkedPlayer(link.RiotId, link.Puuid));
    }
}
