namespace Web.Api.Features.RiotAccount;

// Body of POST /api/me/riot/challenge, e.g. { "riotId": "Nick#EUNE" }
public record StartRiotLinkRequest(string? RiotId);
