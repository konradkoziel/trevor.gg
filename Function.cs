using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using trevor.Model;

namespace trevor;

public class Function
{
    private readonly ILogger<Function> _logger;
    private readonly IAuthentication _auth;

    public Function(ILogger<Function> logger, IAuthentication auth)
    {
        _logger = logger;
        _auth = auth;
    }

    [Function("DiscordCommands")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        FunctionContext context)
    {
        var log = context.GetLogger("DiscordCommands");

        try
        {
            //bool isValid = _auth.VerifyRequestAsync(req, _logger).Result;
            //if (!isValid)
            //{
            //    log.LogWarning("Invalid Discord signature.");
            //    return new StatusCodeResult(401);
            //}

            var interaction = await JsonSerializer.DeserializeAsync<DiscordInteraction>(req.Body, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});


            if(interaction?.Type == 1)
            {
                log.LogInformation("Received Discord PING request.");
                return new ContentResult
                {
                    Content = "{\"type\":1}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            if (Enum.TryParse<Command>(interaction?.Data?.Name, true, out var cmd) && Enum.IsDefined(typeof(Command), cmd))
            {
                return MessageResponse.CreateResponse(cmd, interaction);

            }
            else
            {
                _logger.LogInformation($"Unknown command: {interaction?.Data?.Name}");
                return new OkResult();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception: {ex.Message}");
            return new StatusCodeResult(400);
            
        }
    }
}
