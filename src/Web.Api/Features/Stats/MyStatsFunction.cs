using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.Ranks;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Stats;

// GET /api/me/stats: champions, winrate and recent games of the linked League account
public class MyStatsFunction(
    ISessionTokens sessions, IRiotLinkStore links, IRiotApiClient riot, MatchHistory history, IDataDragon dataDragon)
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

        var link = await links.GetLinkAsync(user.DiscordId, ct);
        if (link is null)
            return new NotFoundObjectResult(new ApiError(NotLinked));

        return await RiotCalls.RunAsync(riot, async () =>
        {
            var matches = await history.RecentAsync(link.Puuid, ct);
            var icons = new Dictionary<string, string?>();
            foreach (var champion in matches.Select(m => m.ChampionName).Distinct())
                icons[champion] = await dataDragon.ChampionIconUrlAsync(champion, ct);
            var ranks = RankedStandings.From(await riot.GetLeagueEntriesAsync(link.Puuid, ct));
            return new OkObjectResult(StatsCalculator.Calculate(link.RiotId, matches, icons) with { Ranks = ranks });
        });
    }
}
