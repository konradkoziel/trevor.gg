namespace Web.Api.Features.RiotAccount;

// GET /api/me/riot: the linked account, or null
public record RiotAccountView(string? LinkedRiotId);
