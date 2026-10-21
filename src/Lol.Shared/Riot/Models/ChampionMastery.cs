namespace Lol.Shared.Riot.Models;

// Champion-Mastery-V4 ChampionMasteryDto (only the fields we use); ChampionId is the numeric key, see Champion.Key
public record ChampionMastery(int ChampionId, int ChampionLevel, long ChampionPoints);
