namespace Web.Api.Features.Matches;

// One of the ten players of a match, with the build they ended the game with
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
    MatchIcon? SecondaryTree);
