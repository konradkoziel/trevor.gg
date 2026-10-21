namespace Web.Api.Features.Builds;

// Mirrors RunePage in src/web/src/features/builds/runes.ts. Primary: index 0 = keystone, 1..3 = minor rows
public record BuildRunes(int? PrimaryTreeId, IReadOnlyList<int?> Primary, int? SecondaryTreeId, IReadOnlyList<SecondaryRune> Secondary);
