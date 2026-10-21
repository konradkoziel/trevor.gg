namespace Web.Api.Features.RiotAccount;

// Profile icons 0-28 are the default ones every League account owns, so anyone can switch to them
public static class VerificationIcons
{
    private const int First = 0;
    private const int Last = 28;

    public static int Pick(int currentIconId, Random random)
    {
        // Never the icon already set, or the check would pass without proving anything
        int icon;
        do icon = random.Next(First, Last + 1);
        while (icon == currentIconId);
        return icon;
    }
}
