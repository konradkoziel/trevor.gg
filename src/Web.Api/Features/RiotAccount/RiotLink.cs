namespace Web.Api.Features.RiotAccount;

// A Discord user's League account
public record RiotLink(string DiscordId, string Puuid, string RiotId, DateTimeOffset LinkedAt);
