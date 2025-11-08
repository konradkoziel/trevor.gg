using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using trevor.Commands;
using trevor.Common;
using trevor.Model;

namespace trevor.Functions
{
    public class CommandFunction
    {
        private readonly ILogger<CommandFunction> _logger;
        private readonly IAuthentication _auth;

        public CommandFunction(ILogger<CommandFunction> logger, IAuthentication auth)
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
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();

                if (!_auth.VerifyRequest(body, req.Headers, log))
                    return new StatusCodeResult(401);

                var interaction = JsonSerializer.Deserialize<DiscordInteraction>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (interaction?.Type == 1) 
                {
                    log.LogInformation("Received Discord PING request.");
                    return new ContentResult
                    {
                        Content = "{\"type\":1}",
                        ContentType = "application/json",
                        StatusCode = 200
                    };
                }

                if (interaction?.Data?.Name != null)
                {
                    CommandFactory commandFactory = new CommandFactory();
                    var command = await commandFactory.Create(interaction.Data.Name, interaction);
                    var result = await command.ExecuteAsync(interaction);
                    var response = MessageResponse.CreateResponse(result);
                    return response;
                }

                log.LogInformation($"Unknown command: {interaction?.Data?.Name}");
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Exception: {ex}");
                return new StatusCodeResult(400);
            }
        }
    }
}
