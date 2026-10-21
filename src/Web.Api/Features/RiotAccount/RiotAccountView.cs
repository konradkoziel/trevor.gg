namespace Web.Api.Features.RiotAccount;

// GET /api/me/riot: the linked account, or the challenge in progress, or neither
public record RiotAccountView(string? LinkedRiotId, RiotChallengeView? Challenge);
