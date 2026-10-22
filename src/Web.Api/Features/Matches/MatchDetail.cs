namespace Web.Api.Features.Matches;

// GET /api/matches/{matchId}: both teams of a finished match, blue side first
public record MatchDetail(string MatchId, string Queue, long DurationSeconds, DateTimeOffset PlayedAt, IReadOnlyList<MatchTeam> Teams);
