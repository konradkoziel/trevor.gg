using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using trevor.Commands.Core;
using trevor.Common;
using trevor.Model;
using static System.Net.WebRequestMethods;

namespace trevor.Functions
{
    public class CommandFunction
    {
        private readonly ILogger<CommandFunction> _logger;
        private readonly IAuthentication _auth;
        private readonly ICommandFactory _commandFactory;
        private static readonly HttpClient _http = new();

        public CommandFunction(ILogger<CommandFunction> logger, IAuthentication auth, ICommandFactory commandFactory)
        {
            _logger = logger;
            _auth = auth;
            _commandFactory = commandFactory;
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
                    var command = await _commandFactory.Create(interaction.Data.Name);
                    var result = await command.ExecuteAsync(interaction);
                    var response = MessageResponse.CreateResponse(result);
                    return response;
                }
                else if (interaction?.Data?.Name == "ask")
                {
                    _ = Task.Run(async () =>
                    {
                        var command = await _commandFactory.Create(interaction.Data.Name);
                        var result = await command.ExecuteAsync(interaction);

                        var followupUrl = $"https://discord.com/api/v10/webhooks/{interaction.ApplicationId}/{interaction.Token}";
                        var payload = new { content = result };
                        var json = JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        await _http.PostAsync(followupUrl, content);
                    });

                    return MessageResponse.CreateDeferredResponse();
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
