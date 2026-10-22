namespace Web.Api.Features.RiotAccount;

// One of a Discord user's League accounts; the main one is what /rank, /roast and "Statystyki" use by default.
// Tracked: the user wants the account's games reported on Trevor's matches channel
public record RiotLink(string DiscordId, string Puuid, string RiotId, DateTimeOffset LinkedAt, bool IsMain = false, bool Tracked = false);
