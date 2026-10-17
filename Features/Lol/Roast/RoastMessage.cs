using System.Globalization;

namespace trevor.Features.Lol.Roast;

// Text of the /roast reply: Discord message, AI input and canned lines
public static class RoastMessage
{
    public static string Format(string displayName, MatchStats stats, string roast) =>
        $"{Header(displayName, stats)}\n{roast}";

    public static string Remake(string displayName, MatchStats stats) =>
        $"{Header(displayName, stats)}\n*Remake. Nawet nie zdążyłeś zawieść, a już po meczu.*";

    public static string NoMatches(string displayName) =>
        $"**{displayName}** nie ma żadnego meczu. Nie da się zroastować kogoś, kto nawet nie gra.";

    // Used when the AI is unavailable
    public static string FallbackRoast(MatchStats s) =>
        s.Deaths > s.Kills + s.Assists
            ? $"*{s.Deaths} śmierci. Wrogowie powinni ci płacić za darmowe złoto.*"
            : s.Win
                ? "*Wygrana. Pewnie cię ktoś wyniósł, ale niech ci będzie.*"
                : "*Przegrana. Nie wiem, co gorsze: twój mecz czy to, że chciałeś, żebym go oceniał.*";

    public static string AiInput(string displayName, MatchStats s) => string.Create(CultureInfo.InvariantCulture,
        $"""
         Gracz: {displayName}
         Postać: {s.Champion}, rola: {s.Role ?? "brak"}, tryb: {s.Queue}
         Wynik: {(s.Win ? "wygrana" : "przegrana")}, czas gry: {s.DurationSeconds / 60} min
         KDA: {s.Kills}/{s.Deaths}/{s.Assists}
         CS: {s.Cs} ({s.CsPerMinute:0.0}/min)
         Obrażenia w bohaterów: {s.Damage} ({s.DamageShare:0%} drużyny, {s.DamageRankInTeam}. miejsce w drużynie)
         Udział w zabójstwach: {s.KillParticipation:0%}
         Wynik wizji: {s.VisionScore}
         Złoto: {s.Gold}
         """);

    private static string Header(string displayName, MatchStats s)
    {
        var role = s.Role is null ? string.Empty : $" ({s.Role})";
        var result = s.Win ? "Wygrana" : "Przegrana";
        return string.Create(CultureInfo.InvariantCulture,
            $"🔥 **{displayName}** — {s.Champion}{role}, {s.Queue}, **{result}** w {s.DurationSeconds / 60} min\n" +
            $"KDA **{s.Kills}/{s.Deaths}/{s.Assists}** · CS {s.Cs} ({s.CsPerMinute:0.0}/min) · " +
            $"DMG {s.Damage / 1000.0:0.0}k ({s.DamageShare:0%} drużyny) · KP {s.KillParticipation:0%} · Wizja {s.VisionScore}");
    }
}
