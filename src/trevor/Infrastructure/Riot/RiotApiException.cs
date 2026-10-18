using System.Net;

namespace trevor.Infrastructure.Riot;

public class RiotApiException(HttpStatusCode statusCode)
    : Exception($"Riot API returned {(int)statusCode}")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
