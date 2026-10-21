namespace Web.Api.Features.RiotAccount;

// A Discord user's verified League account
public record RiotLink(string DiscordId, string Puuid, string RiotId, DateTimeOffset VerifiedAt);
