namespace Lol.Shared.Riot.Models;

// Match-V5 ParticipantDto (only the fields we use); the item, spell, rune and Arena fields are 0/null in older stored copies
public record RiotMatchParticipant(
    string Puuid,
    string ChampionName,
    int TeamId,
    string? TeamPosition,
    bool Win,
    int Kills,
    int Deaths,
    int Assists,
    int TotalMinionsKilled,
    int NeutralMinionsKilled,
    int TotalDamageDealtToChampions,
    int VisionScore,
    int GoldEarned,
    string? RiotIdGameName = null,
    string? RiotIdTagline = null,
    int ChampLevel = 0,
    int Item0 = 0,
    int Item1 = 0,
    int Item2 = 0,
    int Item3 = 0,
    int Item4 = 0,
    int Item5 = 0,
    int Item6 = 0, // the trinket slot
    int Summoner1Id = 0,
    int Summoner2Id = 0,
    RiotMatchPerks? Perks = null,
    int PlayerSubteamId = 0, // Arena's pair (1–8); 0 in every other mode
    int SubteamPlacement = 0) // Arena's final place of that pair (1–8); 0 in every other mode
{
    // The side the player fought on: Arena pairs share TeamId with other pairs, so there it is the pair
    public int Squad() => PlayerSubteamId > 0 ? PlayerSubteamId : TeamId;


    // Inventory slots in order, the trinket last; 0 = empty slot
    public IReadOnlyList<int> ItemIds() => [Item0, Item1, Item2, Item3, Item4, Item5, Item6];
}
