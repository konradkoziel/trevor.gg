namespace Web.Api.Features.RiotAccount;

public record RiotChallengeView(string RiotId, int IconId, string? IconUrl, DateTimeOffset ExpiresAt);
