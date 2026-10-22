using System.Net;
using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.RiotAccount;

// Riot down, rate limited, without a key or with a rejected one → 503 with a message, instead of a 500
public static class RiotCalls
{
    public static async Task<IActionResult> RunAsync(IRiotApiClient riot, Func<Task<IActionResult>> call)
    {
        if (!riot.IsConfigured)
            return Unavailable(RiotAccountMessages.RiotNotConfigured);
        try
        {
            return await call();
        }
        catch (Exception ex) when (ex is RiotApiException or HttpRequestException)
        {
            return Unavailable(ex is RiotApiException { StatusCode: HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized }
                ? RiotAccountMessages.RiotKeyRejected
                : RiotAccountMessages.RiotUnavailable);
        }
    }

    private static ObjectResult Unavailable(string message) =>
        new(new ApiError(message)) { StatusCode = StatusCodes.Status503ServiceUnavailable };
}
