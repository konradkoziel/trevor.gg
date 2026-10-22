using trevor.Core;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Tracking;

// /untrack nick:<Riot ID> — stop reporting the player's matches
public class UntrackCommand(IRiotPlayerRunner players, ITrackedPlayerStore store) : ICommand
{
    public string Name => "untrack";

    // Riot and Table Storage calls can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
        players.RunAsync(interaction, UntrackAsync, ct);

    // A player switched on on the website would be added back by the next sync, so they're switched off there
    private async Task<DiscordResponseData> UntrackAsync(RiotPlayer player, CancellationToken ct)
    {
        var tracked = (await store.ListAsync(ct)).FirstOrDefault(t => t.Puuid == player.Puuid);
        if (tracked is { FromWebsite: true })
            return DiscordResponseData.FromText(TrackingMessage.TrackedOnWebsite(player.DisplayName));

        return DiscordResponseData.FromText(tracked is not null && await store.RemoveAsync(player.Puuid, ct)
            ? TrackingMessage.Untracked(player.DisplayName)
            : TrackingMessage.NotTracked(player.DisplayName));
    }
}
