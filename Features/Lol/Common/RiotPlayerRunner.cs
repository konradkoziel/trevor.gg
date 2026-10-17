using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol.Common;

public record RiotPlayer(string Puuid, string DisplayName);

public interface IRiotPlayerRunner
{
    // Resolves the command's "nick" option to a Riot account and runs the action for it.
    // A missing key, a Riot ID without tag, an unknown player and Riot 401/403/429 errors
    // (also from inside the action) become Trevor-style messages instead.
    Task<string> RunAsync(
        DiscordInteraction interaction,
        Func<RiotPlayer, CancellationToken, Task<string>> action,
        CancellationToken ct = default);
}

public class RiotPlayerRunner(IRiotApiClient riot) : IRiotPlayerRunner
{
    public const string NickOption = "nick";

    public async Task<string> RunAsync(
        DiscordInteraction interaction,
        Func<RiotPlayer, CancellationToken, Task<string>> action,
        CancellationToken ct = default)
    {
        if (!riot.IsConfigured)
            return LolMessages.MissingKey;

        if (!RiotId.TryParse(interaction.GetOption(NickOption), out var riotId))
            return LolMessages.MissingTag;

        try
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            if (account is null)
                return LolMessages.NotFound(riotId);

            return await action(new RiotPlayer(account.Puuid, DisplayName(account, riotId)), ct);
        }
        catch (RiotApiException ex) when (LolMessages.ForRiotError(ex) is { } message)
        {
            return message;
        }
    }

    // Riot's spelling of the name wins over what the user typed
    private static string DisplayName(RiotAccount account, RiotId requested) =>
        account.GameName is null ? requested.ToString() : $"{account.GameName}#{account.TagLine}";
}
