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

        var check = LoginState.Finish(req, req.HttpContext.Response, req.Query["state"]);
        if (!check.Valid)
        {
            logger.LogWarning("Discord login rejected: state missing or different");
            return new RedirectResult(SiteRedirects.LoginFailed(settings.SiteUrl));
        }

        // An error after the silent attempt means "consent needed" (first login, new scope): ask once, properly.
        // After the consent screen it means the user clicked "Cancel".
        if (!string.IsNullOrEmpty(req.Query["error"]))
            return check.WasSilent ? AskForConsent(req) : new RedirectResult(SiteRedirects.LoginFailed(settings.SiteUrl));

        var code = req.Query["code"].ToString();
        var identity = string.IsNullOrEmpty(code) ? null : await discord.GetIdentityAsync(code, ct);
        if (identity is null)
            return new RedirectResult(SiteRedirects.LoginFailed(settings.SiteUrl));
        if (settings.IsGuildOnly && identity.Member is null)
            return new RedirectResult(SiteRedirects.NotMember(settings.SiteUrl));

        var user = new SessionUser(identity.User.Id, identity.DisplayName, identity.AvatarUrl(settings.GuildId));
        return new RedirectResult(SiteRedirects.LoggedIn(settings.SiteUrl, await sessions.IssueAsync(user, ct)));
    }

    private RedirectResult AskForConsent(HttpRequest req)
    {
        var state = LoginState.Start(req.HttpContext.Response, silent: false);
        return new RedirectResult(discord.AuthorizeUrl(state, silent: false).AbsoluteUri);
    }
}
