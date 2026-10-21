using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure.Http;

namespace Web.Api.Features.Builds;

// Shared by create and update: read the body, check it, trim the name
public static class BuildRequest
{
    public static async Task<(SavedBuild? Build, IActionResult? Rejection)> ReadAsync(
        HttpRequest req, BuildValidator validator, CancellationToken ct)
    {
        SavedBuild? build;
        try
        {
            build = await req.ReadFromJsonAsync<SavedBuild>(ct);
        }
        // Not JSON, or not sent as application/json
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            build = null;
        }
        if (build is null)
            return (null, new BadRequestResult());

        var validation = await validator.ValidateAsync(build, ct);
        if (validation.DataUnavailable)
            return (null, new StatusCodeResult(StatusCodes.Status503ServiceUnavailable));
        if (validation.Error is not null)
            return (null, new BadRequestObjectResult(new ApiError(validation.Error)));

        return (build with { Name = build.Name.Trim() }, null);
    }
}
