using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Features.Builds;

namespace Web.Api.Features.Bot;

// GET /api/bot/players/{discordId}/builds?champion=Yasuo: the user's builds for a champion, newest first, for /build
public class BotBuildsFunction(IOptions<BotApiOptions> options, IBuildStore builds, IDataDragon dataDragon, IDataDragonCatalog catalog)
{
    // The newest is what /build shows; the rest only tells how many more there are
    public const int MaxCards = 5;

    [Function("BotBuilds")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bot/players/{discordId}/builds")] HttpRequest req,
        string discordId,
        CancellationToken ct)
    {
        if (BotAuth.Reject(req, options.Value) is { } rejection)
            return rejection;

        var champions = await dataDragon.ChampionsAsync(ct);
        var items = await catalog.ItemsAsync(ct);
        var trees = await catalog.RuneTreesAsync(ct);
        var spells = await catalog.SummonerSpellsAsync(ct);
        if (champions is null || items is null || trees is null || spells is null)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        var champion = ChampionNames.Find(champions, req.Query["champion"].ToString());
        if (champion is null)
            return new NotFoundResult();

        var data = new BuildData(items, trees, spells);
        var cards = (await builds.ListByOwnerAsync(discordId, ct))
            .Where(b => b.Build.ChampionId == champion.Id)
            .Take(MaxCards)
            .Select(b => BuildCards.Create(b, champion, data, options.Value.SiteUrl))
            .ToList();
        return new OkObjectResult(cards);
    }
}
