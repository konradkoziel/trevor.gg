namespace Lol.Shared.Riot.Models;

// Id is the Data Dragon id ("SummonerFlash"); Key is the number Match-V5 uses for it (summoner1Id: 4)
public record SummonerSpell(string Id, string Name, string Description, string IconUrl, int Key = 0);
