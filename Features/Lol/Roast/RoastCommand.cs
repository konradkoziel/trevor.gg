using trevor.Core;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol.Roast;

// /roast nick:<Riot ID> — stats of the player's last match plus Trevor's AI roast
public class RoastCommand(IRiotPlayerRunner players, IRiotApiClient riot, ITrevorAi ai) : ICommand
{
    private const int RoastMaxTokens = 220;

    // Riot counts games under ~3.5 min as remakes; 5 min keeps early surrenders out too
    private const int RemakeSeconds = 300;

    public string Name => "roast";
    public bool IsDeferred => true;

    public Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
        players.RunAsync(interaction, RoastLastMatchAsync, ct);

    private async Task<string> RoastLastMatchAsync(RiotPlayer player, CancellationToken ct)
    {
        var stats = await LastMatchStatsAsync(player.Puuid, ct);
        if (stats is null)
            return RoastMessage.NoMatches(player.DisplayName);

        if (stats.DurationSeconds < RemakeSeconds)
            return RoastMessage.Remake(player.DisplayName, stats);

        var roast = await ai.ReplyAsync(TrevorPrompts.MatchRoast, RoastMessage.AiInput(player.DisplayName, stats), RoastMaxTokens, ct)
                    ?? RoastMessage.FallbackRoast(stats);
        return RoastMessage.Format(player.DisplayName, stats, roast);
    }

    private async Task<MatchStats?> LastMatchStatsAsync(string puuid, CancellationToken ct)
    {
        var matchIds = await riot.GetRecentMatchIdsAsync(puuid, count: 1, ct);
        if (matchIds.Count == 0)
            return null;

        var match = await riot.GetMatchAsync(matchIds[0], ct);
        return match is null ? null : MatchStats.For(match, puuid);
    }
}
