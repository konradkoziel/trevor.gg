using Microsoft.Extensions.Options;
using Lol.Shared.Riot;
using trevor.Core;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Tracking;

// /track nick:<Riot ID> — the player's next matches get reported on the matches channel. An account linked on the website
// gets its "Raporty na Discordzie" switch turned on too, so the site and the bot agree
public class TrackCommand(
    IRiotPlayerRunner players, IRiotApiClient riot, ITrackedPlayerStore store, IWebApiClient web, IOptions<DiscordOptions> options)
    : ICommand
{
    // Every 5-min check makes one Riot call per player; 20 stays far under the personal key's 100 requests / 2 min
    public const int MaxTrackedPlayers = 20;

    public string Name => "track";

    // Riot and Table Storage calls can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
        players.RunAsync(interaction, TrackAsync, ct);

    private async Task<DiscordResponseData> TrackAsync(RiotPlayer player, CancellationToken ct)
    {
        var tracked = await store.ListAsync(ct);
        if (tracked.Any(t => t.Puuid == player.Puuid))
            return DiscordResponseData.FromText(TrackingMessage.AlreadyTracked(player.DisplayName));
        if (tracked.Count >= MaxTrackedPlayers)
            return DiscordResponseData.FromText(TrackingMessage.TooMany(MaxTrackedPlayers));

        // A switched-on website account is then kept in step by TrackedPlayerSync, like one switched on on the site
        var onWebsite = await web.SetTrackedAsync(player.DisplayName, tracked: true, ct) == WebTracking.Switched;

        // Start from the current last match, so the channel doesn't get a game played before /track
        var latest = (await riot.GetRecentMatchIdsAsync(player.Puuid, count: 1, ct)).FirstOrDefault();
        await store.AddAsync(new TrackedPlayer(player.Puuid, player.DisplayName, latest, onWebsite), ct);

        var hasChannel = !string.IsNullOrWhiteSpace(options.Value.MatchesWebhookUrl);
        return DiscordResponseData.FromText(TrackingMessage.Tracking(player.DisplayName, hasChannel));
    }
}
