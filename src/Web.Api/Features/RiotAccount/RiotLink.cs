namespace Web.Api.Features.RiotAccount;

// One of a Discord user's League accounts; the main one is what /rank, /roast and "Statystyki" use by default
public record RiotLink(string DiscordId, string Puuid, string RiotId, DateTimeOffset LinkedAt, bool IsMain = false);
