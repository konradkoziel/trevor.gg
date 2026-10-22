using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Tracking;

// Mirrors the website's "reports on Discord" switches into the tracked players: switched-on accounts are added,
// switched-off ones removed. Players added with /track are left alone. Accounts are matched by Riot ID and looked up
// again here, because PUUIDs differ per Riot API key
public class TrackedPlayerSync(IWebApiClient web, IRiotApiClient riot, ITrackedPlayerStore store, ILogger<TrackedPlayerSync> logger)
{
    public async Task SyncAsync(CancellationToken ct = default)
    {
        if (!riot.IsConfigured)
            return;

        // Null when the site can't answer: nobody is removed on a guess
        var wanted = await web.GetTrackedPlayersAsync(ct);
        if (wanted is null)
            return;

        var tracked = (await store.ListAsync(ct)).ToList();
        foreach (var player in tracked.Where(t => t.FromWebsite && !wanted.Any(w => SameRiotId(w.RiotId, t.RiotId))).ToList())
        {
            await store.RemoveAsync(player.Puuid, ct);
            tracked.Remove(player);
        }

        foreach (var player in wanted.Where(w => !tracked.Any(t => SameRiotId(t.RiotId, w.RiotId))))
        {
            if (tracked.Count >= TrackCommand.MaxTrackedPlayers)
            {
                logger.LogWarning("Tracking limit of {Max} players reached; website accounts left out", TrackCommand.MaxTrackedPlayers);
                return;
            }
            if (await AddAsync(player.RiotId, tracked, ct) is { } added)
                tracked.Add(added);
        }
    }

    // Starts from the current last match, so the channel doesn't get games played before the switch went on.
    // A Riot error skips the player until the next run
    private async Task<TrackedPlayer?> AddAsync(string riotIdText, IReadOnlyList<TrackedPlayer> tracked, CancellationToken ct)
    {
        if (!RiotId.TryParse(riotIdText, out var riotId))
            return null;
        try
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            // Unknown now (renamed since linking), or already tracked under another spelling
            if (account is null || tracked.Any(t => t.Puuid == account.Puuid))
                return null;

            var latest = (await riot.GetRecentMatchIdsAsync(account.Puuid, count: 1, ct)).FirstOrDefault();
            var player = new TrackedPlayer(account.Puuid, riotIdText, latest, FromWebsite: true);
            await store.AddAsync(player, ct);
            return player;
        }
        catch (RiotApiException ex)
        {
            logger.LogWarning(ex, "Could not start tracking a website account");
            return null;
        }
    }

    private static bool SameRiotId(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
