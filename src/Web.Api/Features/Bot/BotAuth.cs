using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Bot;

// Endpoints under /api/bot/ answer only the bot: it sends the shared key in this header
public static class BotAuth
{
    public const string KeyHeader = "X-Trevor-Key";

    // Null when the caller is the bot, else the answer to send
    public static IActionResult? Reject(HttpRequest req, BotApiOptions options)
    {
        if (!options.IsConfigured)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        var sent = req.Headers[KeyHeader].ToString();
        var matches = sent.Length > 0 && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sent), Encoding.UTF8.GetBytes(options.Key));
        return matches ? null : new UnauthorizedResult();
    }
}
