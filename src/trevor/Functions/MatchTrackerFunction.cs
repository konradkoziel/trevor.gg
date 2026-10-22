using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using trevor.Features.Lol.Tracking;

namespace trevor.Functions;

// Every 5 minutes: takes over who has reports switched on on the website, then reports new matches of the tracked players
public class MatchTrackerFunction(ILogger<MatchTrackerFunction> logger, TrackedPlayerSync sync, MatchTracker tracker)
{
    [Function("MatchTracker")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer, CancellationToken ct)
    {
        await sync.SyncAsync(ct);
        var posted = await tracker.PostNewMatchesAsync(ct);
        if (posted > 0)
            logger.LogInformation("Posted {Count} match reports", posted);
    }
}
