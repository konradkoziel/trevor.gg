using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using trevor.Commands.Core;
using trevor.Common;
using trevor.Model;

namespace trevor.Functions
{
    public class CommandFunction(ILogger<CommandFunction> logger, IAuthentication auth, ICommandFactory commandFactory, ICommandHandler commandHandler)
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull

        };
        
        [Function("DiscordCommands")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();

                if (!auth.VerifyRequest(body, req.Headers, logger))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteStringAsync("Invalid signature");
                    return unauthorized;
                }
                
                var interaction = JsonSerializer.Deserialize<DiscordInteraction>(
                    body,
                    _jsonSerializerOptions
                );
                var response = req.CreateResponse();
                if (interaction?.Data?.Name != null)
                {
                    var command = await commandFactory.Create(interaction.Data.Name);
                    var discordResponse = await commandHandler.HandleCommandAsync(command, interaction, _jsonSerializerOptions);
                    response.StatusCode = HttpStatusCode.OK;
                    await response.WriteAsJsonAsync(discordResponse);
                    logger.LogInformation("Received command: {DataName}", interaction?.Data?.Name);
                }
   
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync("No command found.");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError("Exception: {Exception}", ex);
                var error = req.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteStringAsync("Error processing command");
                return error;
            }
        }
    }
}