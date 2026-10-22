namespace Lol.Shared.Riot.Models;

// Match-V5 PerkStyleDto: Style = rune tree id, Selections = the runes taken from it (the keystone first)
public record RiotPerkStyle(int Style, IReadOnlyList<RiotPerkSelection> Selections);
