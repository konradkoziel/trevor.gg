namespace Web.Api.Features.Builds;

// Server-side copy of src/web/src/features/builds/skillOrder.ts: the site never lets these through,
// a hand-made request might
public static class SkillOrderRules
{
    public const int MaxLevel = 18;

    private static readonly Dictionary<string, int> MaxRank = new() { ["Q"] = 5, ["W"] = 5, ["E"] = 5, ["R"] = 3 };

    // The ultimate unlocks its ranks at these levels
    private static readonly int[] UltimateLevels = [6, 11, 16];

    public static bool IsValid(IReadOnlyList<string?> order)
    {
        if (order.Count != MaxLevel)
            return false;

        var ranks = MaxRank.Keys.ToDictionary(k => k, _ => 0);
        for (var index = 0; index < order.Count; index++)
        {
            var key = order[index];
            if (key is null)
                continue;
            if (!ranks.ContainsKey(key))
                return false;

            var level = index + 1;
            var rank = ++ranks[key];
            var allowed = key == "R" ? UltimateLevels.Count(l => l <= level) : (level + 1) / 2;
            if (rank > MaxRank[key] || rank > allowed)
                return false;
        }
        return true;
    }
}
