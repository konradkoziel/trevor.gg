using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Common;

public interface IRiotPlayerRunner
{
    // Resolves the command's "nick" option to a Riot account and runs the action for it.
    // A missing key, a Riot ID without tag, an unknown player and Riot 401/403/429 errors
    // (also from inside the action) become Trevor-style messages instead.
    Task<DiscordResponseData> RunAsync(
        DiscordInteraction interaction,
        Func<RiotPlayer, CancellationToken, Task<DiscordResponseData>> action,
        CancellationToken ct = default);
}
