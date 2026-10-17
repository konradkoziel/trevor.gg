using trevor.Core;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol.Rank;

// /rank nick:<Riot ID> — Solo/Duo and Flex rank with Trevor's AI verdict
public class RankCommand(IRiotPlayerRunner players, IRiotApiClient riot, ITrevorAi ai) : ICommand
{
    private const int VerdictMaxTokens = 120;

    public string Name => "rank";

    // Riot calls (plus retries on 429) and an AI call can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
        players.RunAsync(interaction, ReportRankAsync, ct);

    private async Task<string> ReportRankAsync(RiotPlayer player, CancellationToken ct)
    {
        var ranks = PlayerRanks.From(await riot.GetLeagueEntriesAsync(player.Puuid, ct));
        if (ranks.IsUnranked)
            return RankMessage.Unranked(player.DisplayName);

        var verdict = await ai.ReplyAsync(TrevorPrompts.RankVerdict, RankMessage.AiInput(player.DisplayName, ranks), VerdictMaxTokens, ct)
                      ?? RankMessage.FallbackVerdict(ranks.Main!.Tier);
        return RankMessage.Format(player.DisplayName, ranks, verdict);
    }
}
