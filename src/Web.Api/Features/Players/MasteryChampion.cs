namespace Web.Api.Features.Players;

// One of a player's most played champions by Champion-Mastery-V4 points
public record MasteryChampion(string ChampionId, string Name, string? IconUrl, int Level, long Points);
