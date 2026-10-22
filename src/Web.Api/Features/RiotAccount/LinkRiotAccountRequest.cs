namespace Web.Api.Features.RiotAccount;

// Body of POST /api/me/riot, e.g. { "riotId": "Nick#EUNE" }
public record LinkRiotAccountRequest(string? RiotId);
