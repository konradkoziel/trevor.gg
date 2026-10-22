namespace Lol.Shared.Riot.Models;

// Match-V5 PerksDto: the primary rune tree first, then the secondary one
public record RiotMatchPerks(IReadOnlyList<RiotPerkStyle> Styles);
