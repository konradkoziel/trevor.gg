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

    private async Task<DiscordResponseData> UntrackAsync(RiotPlayer player, CancellationToken ct) =>
        DiscordResponseData.FromText(await store.RemoveAsync(player.Puuid, ct)
            ? TrackingMessage.Untracked(player.DisplayName)
            : TrackingMessage.NotTracked(player.DisplayName));
}
