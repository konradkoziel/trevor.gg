namespace trevor.Features.Lol.Tracking;

// A player whose matches are reported on the matches channel; LastMatchId is the newest one already handled
public record TrackedPlayer(string Puuid, string RiotId, string? LastMatchId);
