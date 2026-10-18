using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol.Tracking;

// Finds tracked players' new matches and posts one short report per match to the matches channel
public class MatchTracker(
    ITrackedPlayerStore store,
    IRiotApiClient riot,
    ITrevorAi ai,
    IDiscordApiClient discord,
    IOptions<DiscordOptions> options,
    ILogger<MatchTracker> logger)
{
    private const int CommentMaxTokens = 100;

    // Returns how many reports were posted
    public async Task<int> PostNewMatchesAsync(CancellationToken ct = default)
    {
        var webhookUrl = options.Value.MatchesWebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl) || !riot.IsConfigured)
        {
            logger.LogInformation("Match tracking off: DISCORD_URL_MATCHES or RIOT_API_KEY missing");
            return 0;
        }

        var newMatches = await FindNewMatchesAsync(await store.ListAsync(ct), ct);
        var posted = 0;
        foreach (var (matchId, players) in newMatches)
        {
            if (await ReportAsync(webhookUrl, matchId, players, ct))
                posted++;
        }
        return posted;
    }

    // Friends who played together share the match id, so they end up in one report
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

    // The match is marked as handled only after the post went out, so a failed post is retried on the next run
    private async Task<bool> ReportAsync(string webhookUrl, string matchId, List<TrackedPlayer> players, CancellationToken ct)
    {
        try
        {
            var match = await riot.GetMatchAsync(matchId, ct);
            var report = match is null ? null : MatchReport.For(match, players);
            var posted = false;
            if (report is { IsRemake: false })
            {
                await discord.SendWebhookAsync(webhookUrl, MatchReportMessage.Format(report, await CommentAsync(report, ct)), ct);
                posted = true;
            }

            foreach (var player in players)
                await store.SetLastMatchAsync(player.Puuid, matchId, ct);
            return posted;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Report of match {MatchId} failed", matchId);
            return false;
        }
    }

    private async Task<string> CommentAsync(MatchReport report, CancellationToken ct) =>
        await ai.ReplyAsync(TrevorPrompts.MatchSummary, MatchReportMessage.AiInput(report), CommentMaxTokens, ct)
        ?? MatchReportMessage.FallbackComment(report);
}
