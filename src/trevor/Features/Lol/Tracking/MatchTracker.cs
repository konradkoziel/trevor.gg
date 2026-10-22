using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lol.Shared.Riot;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Tracking;

// Finds tracked players' new matches and posts one short report per match to the matches channel
public class MatchTracker(
    ITrackedPlayerStore store,
    IReportedMatchStore reported,
    IRiotApiClient riot,
    IDataDragon dataDragon,
    IDiscordApiClient discord,
    IOptions<DiscordOptions> options,
    IOptions<WebApiOptions> web,
    ILogger<MatchTracker> logger)
{
    // Returns how many reports were posted
    public async Task<int> PostNewMatchesAsync(CancellationToken ct = default)
    {
        var webhookUrl = options.Value.MatchesWebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl) || !riot.IsConfigured)
        {
            logger.LogInformation("Match tracking off: DISCORD_MATCHES_WEBHOOK or RIOT_API_KEY missing");
            return 0;
        }

        var tracked = await store.ListAsync(ct);
        var posted = 0;
        foreach (var (matchId, surfaced) in await FindNewMatchesAsync(tracked, ct))
        {
            if (await HandleAsync(webhookUrl, matchId, tracked, surfaced, ct))
                posted++;
        }

        logger.LogInformation("Checked {Players} tracked players, posted {Reports} match reports", tracked.Count, posted);
        return posted;
    }

    // New match id → the tracked players whose latest match it is. Riot may list a shared match
    // for one friend minutes before another, so this is only a hint of who played; the match itself decides
    private async Task<Dictionary<string, List<TrackedPlayer>>> FindNewMatchesAsync(IReadOnlyList<TrackedPlayer> players, CancellationToken ct)
    {
        var byMatch = new Dictionary<string, List<TrackedPlayer>>();
        foreach (var player in players)
        {
            var latest = await LatestMatchIdAsync(player, ct);
            if (latest is null || latest == player.LastMatchId)
                continue;

            if (!byMatch.TryGetValue(latest, out var group))
                byMatch[latest] = group = [];
            group.Add(player);
        }
        return byMatch;
    }

    // A Riot error for one player shouldn't stop the others; they're retried on the next run
    private async Task<string?> LatestMatchIdAsync(TrackedPlayer player, CancellationToken ct)
    {
        try
        {
            var ids = await riot.GetRecentMatchIdsAsync(player.Puuid, count: 1, ct);
            return ids.Count == 0 ? null : ids[0];
        }
        catch (RiotApiException ex)
        {
            logger.LogWarning(ex, "Could not check new matches of tracked player {Puuid}", player.Puuid);
            return null;
        }
    }

    // One card per match, listing every tracked player in it. The match is marked as handled only after
    // the post went out, so a failed post is retried on the next run
    private async Task<bool> HandleAsync(
        string webhookUrl, string matchId, IReadOnlyList<TrackedPlayer> tracked, List<TrackedPlayer> surfaced, CancellationToken ct)
    {
        try
        {
            var posted = !await reported.ContainsAsync(matchId, ct) && await PostAsync(webhookUrl, matchId, tracked, ct);

            foreach (var player in surfaced)
                await store.SetLastMatchAsync(player.Puuid, matchId, ct);
            return posted;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Report of match {MatchId} failed", matchId);
            return false;
        }
    }

    // False for remakes and matches Riot no longer has; those are marked as reported all the same
    private async Task<bool> PostAsync(string webhookUrl, string matchId, IReadOnlyList<TrackedPlayer> tracked, CancellationToken ct)
    {
        var match = await riot.GetMatchAsync(matchId, ct);
        var report = match is null ? null : MatchReport.For(match, tracked);
        if (report is { IsRemake: false })
        {
            var icon = await dataDragon.ChampionIconUrlAsync(report.Mvp.Champion, ct);
            var matchUrl = SiteLinks.Match(web.Value.SiteUrl, matchId);
            await discord.SendWebhookAsync(webhookUrl, MatchReportMessage.Format(report, icon, matchUrl), ct);
        }

        await reported.AddAsync(matchId, ct);
        return report is { IsRemake: false };
    }
}
