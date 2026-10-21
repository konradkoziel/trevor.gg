namespace Web.Api.Features.Bot;

// GET /api/bot/players/{discordId}: the League account a Discord user linked on the site
public record LinkedPlayer(string RiotId, string Puuid);
