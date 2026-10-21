using System.Globalization;
using System.Text;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Bot;

// What people type in Discord: "yasuo", "Kai'Sa", "kaisa", "wukong", "MonkeyKing" all find their champion
public static class ChampionNames
{
    public static Champion? Find(IReadOnlyList<Champion> champions, string? typed)
    {
        var needle = Normalize(typed);
        return needle.Length == 0 ? null : champions.FirstOrDefault(c => Normalize(c.Name) == needle || Normalize(c.Id) == needle);
    }

    // Letters and digits only, no accents, lower case
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        var decomposed = text.Normalize(NormalizationForm.FormD);
        return new string(decomposed.Where(c => char.IsLetterOrDigit(c) && CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(char.ToLowerInvariant).ToArray());
    }
}
