using System.Globalization;
using System.Net;
using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol;

// /rank nick:Konrad#EUNE — Solo/Duo and Flex rank from Riot API, with Trevor's verdict
public class RankCommand(IRiotApiClient riot) : ICommand
{
    public string Name => "rank";

    // Two Riot calls (plus retries on 429) can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public async Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        if (!riot.IsConfigured)
            return "Nie mam klucza do Riot API. Ktoś tu miał go dodać i jak zwykle zapomniał.";

        var nick = interaction.GetOption("nick");
        if (!RiotId.TryParse(nick, out var riotId))
            return "Podaj nick z tagiem, np. `Konrad#EUNE`. Tag znajdziesz w kliencie pod swoim nickiem.";

        try
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            if (account is null)
                return $"Nie ma gracza **{riotId}**. Nawet Riot o tobie nie słyszał.";

            var entries = await riot.GetLeagueEntriesAsync(account.Puuid, ct);
            var displayName = account.GameName is null ? riotId.ToString() : $"{account.GameName}#{account.TagLine}";
            return Format(displayName, entries);
        }
        catch (RiotApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return "Riot mnie dusi limitem zapytań. Spróbuj za minutę.";
        }
        catch (RiotApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "Riot odrzucił mój klucz. Pewnie wygasł — deweloperskie żyją 24 h. Ktoś musi wgrać nowy.";
        }
    }

    public static string Format(string displayName, IReadOnlyList<LeagueEntry> entries)
    {
        var solo = entries.FirstOrDefault(e => e.QueueType == LeagueEntry.SoloQueue);
        var flex = entries.FirstOrDefault(e => e.QueueType == LeagueEntry.FlexQueue);

        if (solo is null && flex is null)
            return $"🏅 **{displayName}** — unranked.\n*Zero rankedów. Boisz się, że wyjdzie na jaw, jaki jesteś słaby?*";

        var lines = new List<string> { $"🏅 **{displayName}**" };
        if (solo is not null)
            lines.Add($"Solo/Duo: {Describe(solo)}");
        if (flex is not null)
            lines.Add($"Flex: {Describe(flex)}");
        lines.Add($"*{Verdict((solo ?? flex)!.Tier)}*");

        return string.Join("\n", lines);
    }

    private static string Describe(LeagueEntry entry)
    {
        var games = entry.Wins + entry.Losses;
        var winrate = games == 0 ? 0 : 100.0 * entry.Wins / games;
        var division = IsApexTier(entry.Tier) ? string.Empty : $" {entry.Rank}";
        return string.Create(CultureInfo.InvariantCulture,
            $"**{TierName(entry.Tier)}{division}, {entry.LeaguePoints} LP** — {entry.Wins}W / {entry.Losses}L ({winrate:0.0}%)");
    }

    private static bool IsApexTier(string tier) => tier is "MASTER" or "GRANDMASTER" or "CHALLENGER";

    private static string TierName(string tier) =>
        tier.Length == 0 ? tier : tier[0] + tier[1..].ToLowerInvariant();

    private static string Verdict(string tier) => tier switch
    {
        "IRON" => "Iron. Nawet boty z intro do gry by cię wyniosły.",
        "BRONZE" => "Bronze. Tu się nie gra, tu się cierpi.",
        "SILVER" => "Silver. Ani dobry, ani zły. Po prostu nikt.",
        "GOLD" => "Gold. Gratuluję, jesteś idealnie przeciętny.",
        "PLATINUM" => "Platinum. Myślisz, że umiesz grać. Prawie.",
        "EMERALD" => "Emerald. Zielony jak twoje pojęcie o makro.",
        "DIAMOND" => "Diamond. Dobra, coś tam umiesz. Nie przyzwyczajaj się do pochwał.",
        "MASTER" or "GRANDMASTER" => "Master+. Wyjdź na dwór, serio.",
        "CHALLENGER" => "Challenger. Co ty robisz na tym serwerze z tymi noobami?",
        _ => "Nie wiem, co to za ranga, ale i tak jesteś słaby."
    };
}
