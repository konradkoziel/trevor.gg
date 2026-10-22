using Web.Api.Features.Builds;
using Web.Api.Features.Stats;

namespace Web.Api.Features.Players;

// GET /api/players/{name}/{tag}: everything the site knows about a linked friend
public record PlayerProfile(
    string RiotId,
    string? ProfileIconUrl,
    long Level,
    IReadOnlyList<MasteryChampion> TopChampions,
    PlayerStats Stats,
    IReadOnlyList<BuildSummary> Builds,
    // The same person's other linked accounts, main first
    IReadOnlyList<string>? OtherAccounts = null);
