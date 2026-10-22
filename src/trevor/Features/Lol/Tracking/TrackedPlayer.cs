namespace trevor.Features.Lol.Tracking;

// A player whose matches are reported on the matches channel; LastMatchId is the newest one already handled.
// FromWebsite: switched on on the website (TrackedPlayerSync adds and removes them), else added with /track
public record TrackedPlayer(string Puuid, string RiotId, string? LastMatchId, bool FromWebsite = false);
