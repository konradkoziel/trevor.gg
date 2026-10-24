using Lol.Shared.Riot;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Builds;
using Web.Api.Features.Home;

namespace Web.Api.Features.WarmUp;

// Every 5 minutes: keeps an instance of the API awake, since waking .NET up on Flex Consumption costs a visitor
// seconds, and gets Data Dragon, the rotation and the leaderboard ready before anyone has to wait for them
public class WarmUpFunction(IRiotApiClient riot, IDataDragon dataDragon, IDataDragonCatalog catalog, FreeRotation rotation,
    Leaderboard leaderboard, ILogger<WarmUpFunction> logger)
{
    [Function("WarmUp")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer, CancellationToken ct)
    {
        // Data Dragon calls are null when it is down, never exceptions
        await dataDragon.ChampionsAsync(ct);
        await BuildDataSource.LoadAsync(catalog, ct);
        if (!riot.IsConfigured)
            return;

        try
        {
            await rotation.ChampionsAsync(ct);
            await leaderboard.WarmAsync();
        }
        catch (Exception ex) when (ex is RiotApiException or HttpRequestException)
        {
            // Visitors get the same 503 they would have got anyway; the next run tries again
            logger.LogWarning(ex, "Warm-up: Riot failed");
        }
    }
}
