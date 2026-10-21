namespace Lol.Shared.Riot.Models;

// Precision, Domination, ...: Slots[0] holds the keystones, Slots[1..3] the minor runes
public record RuneTree(int Id, string Name, string IconUrl, IReadOnlyList<IReadOnlyList<Rune>> Slots);
