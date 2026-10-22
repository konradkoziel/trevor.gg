namespace trevor.Features.Lol.Tracking;

// Replies of /track and /untrack
public static class TrackingMessage
{
    public const string NoChannel =
        "\nTylko że nie mam kanału na mecze (`DISCORD_MATCHES_WEBHOOK`). Dopóki ktoś go nie ustawi, nic nie wrzucę.";

    public static string Tracking(string name, bool hasChannel) =>
        $"Od teraz każdy mecz **{name}** ląduje na kanale z meczami. Nie ma ucieczki.{(hasChannel ? string.Empty : NoChannel)}";

    public static string AlreadyTracked(string name) => $"**{name}** już jest śledzony. Paranoja?";

    public static string TooMany(int max) =>
        $"Śledzę już {max} graczy. Więcej Riot mi nie da — wywal kogoś przez `/untrack`.";

    public static string Untracked(string name) => $"Przestaję śledzić **{name}**. Kolejne porażki zostaną prywatne.";

    public static string TrackedOnWebsite(string name) =>
        $"Strona nie odpowiada, a **{name}** ma tam włączone raporty. Wyłącz je w zakładce Konto, bo inaczej za pięć minut wrócę jak bumerang.";

    public static string NotTracked(string name) => $"Nie śledzę **{name}**. Nikt by nie chciał.";
}
