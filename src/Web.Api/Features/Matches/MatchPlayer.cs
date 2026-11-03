namespace Web.Api.Features.Matches;

// One player of a match, with the build they ended the game with
public record MatchPlayer(
    string? RiotId,
    string ChampionName,
    string? ChampionIconUrl,
    int ChampionLevel,
    int Kills,
    int Deaths,
    int Assists,
    int CreepScore,
    int Damage,
    int Gold,
    int VisionScore,
    // Six inventory slots then the trinket; null = empty slot
    IReadOnlyList<MatchIcon?> Items,
    IReadOnlyList<MatchIcon> Spells,
    MatchIcon? Keystone,
    MatchIcon? SecondaryTree,
    // PerformanceScore against the team's average: 1 = an average teammate
    double Rating = 1,
    // "mvp" (best of the team) or "report" (worst of the team); null for the rest and in Arena pairs
    string? Award = null);
