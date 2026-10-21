using System.Net;
using System.Text.RegularExpressions;

namespace Lol.Shared.Riot;

// Data Dragon descriptions are HTML-ish ("<mainText><stats>50 <attention>AD</attention></stats><br>...");
// the site shows them as plain text
public static partial class DataDragonText
{
    public static string Plain(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = LineBreak().Replace(html, "\n");
        text = Tag().Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = BlankLines().Replace(text, "\n");
        return text.Trim();
    }

    [GeneratedRegex(@"<br\s*/?>|</?(p|li|passive|active|stats|mainText)>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreak();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"\s*\n\s*")]
    private static partial Regex BlankLines();
}
