namespace Lol.Shared.Riot.Models;

// Match-V5 InfoDto (only the fields we use)
public record RiotMatchInfo(
    long GameDuration, // seconds
    int QueueId,
    string GameMode,
    IReadOnlyList<RiotMatchParticipant> Participants,
    long GameEndTimestamp = 0); // Unix ms; 0 when Riot leaves it out
