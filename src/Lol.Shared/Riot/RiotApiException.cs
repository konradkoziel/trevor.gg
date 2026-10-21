using System.Net;

namespace Lol.Shared.Riot;

public class RiotApiException(HttpStatusCode statusCode)
    : Exception($"Riot API returned {(int)statusCode}")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
