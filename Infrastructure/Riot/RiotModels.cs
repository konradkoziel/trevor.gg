namespace trevor.Infrastructure.Riot;

// Riot ID, e.g. "Nick#EUNE" → GameName "Nick", TagLine "EUNE"
public record RiotId(string GameName, string TagLine)
{
    public static bool TryParse(string? text, out RiotId id)
    {
        id = new RiotId(string.Empty, string.Empty);
        var value = text?.Trim();
        var hash = value?.LastIndexOf('#') ?? -1;
        if (value is null || hash <= 0 || hash == value.Length - 1)
            return false;

        id = new RiotId(value[..hash].Trim(), value[(hash + 1)..].Trim());
        return id.GameName.Length > 0 && id.TagLine.Length > 0;
    }

    public override string ToString() => $"{GameName}#{TagLine}";
}

// Account-V1 AccountDto
public record RiotAccount(string Puuid, string? GameName, string? TagLine);

// League-V4 LeagueEntryDTO (only the fields we use)
public record LeagueEntry(string QueueType, string Tier, string Rank, int LeaguePoints, int Wins, int Losses)
{
    public const string SoloQueue = "RANKED_SOLO_5x5";
    public const string FlexQueue = "RANKED_FLEX_SR";
}

// Match-V5 MatchDto (only the fields we use)
public record RiotMatch(RiotMatchInfo Info);

public record RiotMatchInfo(
    long GameDuration, // seconds
    int QueueId,
    string GameMode,
    IReadOnlyList<RiotMatchParticipant> Participants);

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
