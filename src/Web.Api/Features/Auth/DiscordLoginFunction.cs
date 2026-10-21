using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Infrastructure.Discord;

namespace Web.Api.Features.Auth;

// GET /api/auth/discord/login: the site's "Log in with Discord" link; sends the browser to Discord's consent page
public class DiscordLoginFunction(IDiscordOAuthClient discord, IOptions<DiscordOAuthOptions> options)
{
    [Function("DiscordLogin")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/discord/login")] HttpRequest req)
    {
        if (!options.Value.IsConfigured)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        var state = LoginState.Start(req.HttpContext.Response);
        return new RedirectResult(discord.AuthorizeUrl(state).AbsoluteUri);
    }
}
