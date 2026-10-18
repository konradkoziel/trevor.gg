namespace trevor.Infrastructure.Riot.Models;

// Match-V5 InfoDto (only the fields we use)
public record RiotMatchInfo(
    long GameDuration, // seconds
    int QueueId,
    string GameMode,
    IReadOnlyList<RiotMatchParticipant> Participants);
