namespace Lol.Shared.Riot.Models;

// Key is "P" for the passive, then "Q", "W", "E", "R"
public record ChampionAbility(string Key, string Name, string Description, string IconUrl);
