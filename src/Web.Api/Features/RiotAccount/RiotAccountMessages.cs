namespace Web.Api.Features.RiotAccount;

public static class RiotAccountMessages
{
    public const string BadRiotId = "Riot ID wygląda tak: Nick#EUNE. Z hashtagiem.";
    public const string NoSuchAccount = "Nie ma takiego konta. Literówka czy konto wyimaginowane?";
    public const string NeverPlayed = "To konto nigdy nie grało w LoLa. Szczęściarz.";
    public const string AlreadyLinked = "To konto podpiął już ktoś inny. Jak to pomyłka, niech je u siebie odepnie.";
    public const string RiotUnavailable = "Riot nie odpowiada. Spróbuj za chwilę.";
    public const string RiotNotConfigured = "Na serwerze brakuje klucza Riot API.";

    // 401/403 from Riot: the key is there but no longer valid, most often an expired 24 h development key
    public const string RiotKeyRejected = "Riot odrzucił klucz API. Pewnie wygasł (deweloperskie żyją 24 h), ktoś musi wgrać nowy.";
}
