namespace trevor.Infrastructure.Web;

// Mirrors Web.Api.Features.Bot.BuildCard
public record WebBuildCard(
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
