using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Web.Api.Infrastructure.Discord;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Auth;

// GET /api/auth/discord/callback: Discord's redirect after "Authorize" (the Redirect URL set in the Developer Portal)
public class DiscordCallbackFunction(
    IDiscordOAuthClient discord,
    ISessionTokens sessions,
    IOptions<DiscordOAuthOptions> options,
    ILogger<DiscordCallbackFunction> logger)
{
    [Function("DiscordCallback")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/discord/callback")] HttpRequest req,
        CancellationToken ct)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        if (!LoginState.Finish(req, req.HttpContext.Response, req.Query["state"]))
        {
            logger.LogWarning("Discord login rejected: state missing or different");
            return new RedirectResult(SiteRedirects.LoginFailed(settings.SiteUrl));
        }

        // Missing when the user clicked "Cancel" on Discord
        var code = req.Query["code"].ToString();
        var user = string.IsNullOrEmpty(code) ? null : await discord.GetUserAsync(code, ct);
        if (user is null)
            return new RedirectResult(SiteRedirects.LoginFailed(settings.SiteUrl));

        var token = await sessions.IssueAsync(new SessionUser(user.Id, user.DisplayName, DiscordAvatars.Url(user.Id, user.Avatar)), ct);
        return new RedirectResult(SiteRedirects.LoggedIn(settings.SiteUrl, token));
    }
}
