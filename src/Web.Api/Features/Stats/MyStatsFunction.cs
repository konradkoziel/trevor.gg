using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Stats;

// GET /api/me/stats?account=: champions, winrate and recent games of one of the user's League accounts
public class MyStatsFunction(ISessionTokens sessions, IRiotLinkStore links, IRiotApiClient riot, StatsReport report)
{
    public const string NotLinked = "Najpierw podepnij konto LoL, Trevor nie zgaduje.";

    [Function("MyStats")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me/stats")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        // ?account=<id> picks one of the user's accounts; the main one otherwise
        var accounts = await links.ListAccountsAsync(user.DiscordId, ct);
        var requested = req.Query["account"].ToString();
        var link = accounts.FirstOrDefault(a => a.Puuid == requested) ?? RiotLinks.MainOf(accounts);
        if (link is null)
            return new NotFoundObjectResult(new ApiError(NotLinked));

        return await RiotCalls.RunAsync(riot, async () => new OkObjectResult(await report.ForAsync(link, ct)));
    }
}
