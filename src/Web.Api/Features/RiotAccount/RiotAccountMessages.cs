namespace Web.Api.Features.RiotAccount;

public static class RiotAccountMessages
{
    public const string BadRiotId = "Riot ID wygląda tak: Nick#EUNE. Z hashtagiem.";
    public const string NoSuchAccount = "Nie ma takiego konta. Literówka czy konto wyimaginowane?";
    public const string NeverPlayed = "To konto nigdy nie grało w LoLa. Szczęściarz.";
    public const string NoChallenge = "Najpierw podaj Riot ID, potem sprawdzaj ikonę.";
    public const string ChallengeExpired = "Minęło za dużo czasu. Zacznij od nowa, dostaniesz nową ikonę.";
    public const string WrongIcon = "Ikona się nie zgadza. Zmień ją w kliencie i odczekaj chwilę, Riot czasem się spóźnia.";
    public const string RiotUnavailable = "Riot nie odpowiada. Spróbuj za chwilę.";
    public const string RiotNotConfigured = "Na serwerze brakuje klucza Riot API.";
}
