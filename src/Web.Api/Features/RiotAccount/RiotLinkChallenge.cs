namespace Web.Api.Features.RiotAccount;

// "Set this profile icon to prove the account is yours", waiting for the user to do it
public record RiotLinkChallenge(string DiscordId, string Puuid, string RiotId, int IconId, DateTimeOffset ExpiresAt);
