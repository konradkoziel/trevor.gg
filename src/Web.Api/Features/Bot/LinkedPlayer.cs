namespace Web.Api.Features.Bot;

// A League account linked on the site: GET /api/bot/players/{discordId} (the main one) and GET /api/bot/tracked
public record LinkedPlayer(string RiotId, string Puuid);
