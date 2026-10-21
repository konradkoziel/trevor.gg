namespace Web.Api.Features.Bot;

// A saved build with names instead of ids, ready for a Discord embed
public record BuildCard(
    string Name,
    string ChampionName,
    string? ChampionIconUrl,
    string OwnerName,
    string Url,
    IReadOnlyList<string> Items,
    string? Keystone,
    IReadOnlyList<string> Runes,
    IReadOnlyList<string> RuneTrees,
    IReadOnlyList<string> SummonerSpells,
    IReadOnlyList<string> SkillMaxOrder);
