using System.Net;
using trevor.Infrastructure.Riot;
using trevor.Infrastructure.Riot.Models;

namespace trevor.Features.Lol.Common;

// Messages shared by every Riot-backed command
public static class LolMessages
{
    // Embed footer and verdict field title of every LoL card
    public const string EmbedFooter = "Trevor.gg · Riot API";
    public const string VerdictTitle = "Werdykt Trevora";

    public const string MissingKey = "Nie mam klucza do Riot API. Ktoś tu miał go dodać i jak zwykle zapomniał.";
    public const string MissingTag = "Podaj nick z tagiem, np. `Nick#EUNE`. Tag znajdziesz w kliencie pod swoim nickiem.";

    public static string NotFound(RiotId riotId) => $"Nie ma gracza **{riotId}**. Nawet Riot o tobie nie słyszał.";

    // Null for errors that should surface as the generic error message
    public static string? ForRiotError(RiotApiException ex) => ex.StatusCode switch
    {
        HttpStatusCode.TooManyRequests => "Riot mnie dusi limitem zapytań. Spróbuj za minutę.",
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
            "Riot odrzucił mój klucz. Pewnie wygasł — deweloperskie żyją 24 h. Ktoś musi wgrać nowy.",
        _ => null
    };
}
