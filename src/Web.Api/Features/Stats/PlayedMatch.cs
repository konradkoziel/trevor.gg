namespace Web.Api.Features.Stats;

// One player's side of one match, kept so each match is fetched from Riot only once
public record PlayedMatch(
    string MatchId,
    string ChampionName,
    bool Win,
    int Kills,
    int Deaths,
    int Assists,
    int CreepScore,
    int DamageToChampions,
    long DurationSeconds,
    int QueueId,
    DateTimeOffset PlayedAt);
