using trevor.Core;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Tracking;

// /untrack nick:<Riot ID> — stop reporting the player's matches; an account linked on the website gets its switch turned off
public class UntrackCommand(IRiotPlayerRunner players, ITrackedPlayerStore store, IWebApiClient web) : ICommand
{
    public string Name => "untrack";

    // Riot and Table Storage calls can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
        players.RunAsync(interaction, UntrackAsync, ct);

    private async Task<DiscordResponseData> UntrackAsync(RiotPlayer player, CancellationToken ct)
    {
        var tracked = (await store.ListAsync(ct)).FirstOrDefault(t => t.Puuid == player.Puuid);
        var website = await web.SetTrackedAsync(player.DisplayName, tracked: false, ct);

        // Still switched on on the site, so the next sync would bring the player straight back
        if (website == WebTracking.Unavailable && tracked is { FromWebsite: true })
            return DiscordResponseData.FromText(TrackingMessage.TrackedOnWebsite(player.DisplayName));

        var removed = tracked is not null && await store.RemoveAsync(player.Puuid, ct);
        return DiscordResponseData.FromText(removed || website == WebTracking.Switched
            ? TrackingMessage.Untracked(player.DisplayName)
            : TrackingMessage.NotTracked(player.DisplayName));
    }
}
