namespace Web.Api.Features.Matches;

// GET /api/matches/{matchId}: the teams of a finished match, blue side first (Arena: every pair, best place first)
public record MatchDetail(string MatchId, string Queue, long DurationSeconds, DateTimeOffset PlayedAt, IReadOnlyList<MatchTeam> Teams);
