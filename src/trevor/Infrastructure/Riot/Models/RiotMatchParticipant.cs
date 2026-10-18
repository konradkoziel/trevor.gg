namespace trevor.Infrastructure.Riot.Models;

// Match-V5 ParticipantDto (only the fields we use)
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
    int GoldEarned);
