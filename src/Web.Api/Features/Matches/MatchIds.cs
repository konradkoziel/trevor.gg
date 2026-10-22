using System.Text.RegularExpressions;

namespace Web.Api.Features.Matches;

// Match-V5 ids look like EUN1_3712345678: a platform, an underscore and a number
public static partial class MatchIds
{
    public static bool IsWellFormed(string? id) => id is not null && Pattern().IsMatch(id);

    [GeneratedRegex("^[A-Z0-9]{2,5}_[0-9]{1,15}$")]
    private static partial Regex Pattern();
}
