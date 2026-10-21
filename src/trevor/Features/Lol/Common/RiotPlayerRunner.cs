using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Common;

public class RiotPlayerRunner(IRiotApiClient riot, IWebApiClient web) : IRiotPlayerRunner
{
    public const string NickOption = "nick";

    public async Task<DiscordResponseData> RunAsync(
        DiscordInteraction interaction,
        Func<RiotPlayer, CancellationToken, Task<DiscordResponseData>> action,
        CancellationToken ct = default)
    {
        if (!riot.IsConfigured)
            return DiscordResponseData.FromText(LolMessages.MissingKey);

        var nick = interaction.GetOption(NickOption);
        if (string.IsNullOrWhiteSpace(nick))
            return await RunForLinkedAccountAsync(interaction, action, ct);

        return RiotId.TryParse(nick, out var riotId)
            ? await RunForAsync(riotId, action, ct)
            : DiscordResponseData.FromText(LolMessages.MissingTag);
    }

    // No nick: the account the user linked on the website
    private async Task<DiscordResponseData> RunForLinkedAccountAsync(
        DiscordInteraction interaction, Func<RiotPlayer, CancellationToken, Task<DiscordResponseData>> action, CancellationToken ct)
    {
        if (!web.IsConfigured || interaction.GetUserId() is not { } userId)
            return DiscordResponseData.FromText(LolMessages.MissingTag);

        var linked = await web.GetLinkedPlayerAsync(userId, ct);
        if (linked is null || !RiotId.TryParse(linked.RiotId, out var riotId))
            return DiscordResponseData.FromText(LolMessages.NotLinked(web.SiteUrl));

        // Looked up by Riot ID again: PUUIDs are encrypted per Riot API key, and the site may use another key
        return await RunForAsync(riotId, action, ct);
    }

    private async Task<DiscordResponseData> RunForAsync(
        RiotId riotId, Func<RiotPlayer, CancellationToken, Task<DiscordResponseData>> action, CancellationToken ct)
    {
        try
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            if (account is null)
                return DiscordResponseData.FromText(LolMessages.NotFound(riotId));

            return await action(new RiotPlayer(account.Puuid, RiotNames.RiotId(account) ?? riotId.ToString()), ct);
        }
        catch (RiotApiException ex) when (LolMessages.ForRiotError(ex) is { } message)
        {
            return DiscordResponseData.FromText(message);
        }
    }
}
