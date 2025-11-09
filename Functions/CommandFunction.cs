using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using trevor.Commands.Core;
using trevor.Common;
using trevor.Model;
using trevor.Model.Response.cs;

namespace trevor.Functions
{
    public class CommandFunction(
        ILogger<CommandFunction> logger,
        IAuthentication auth,
        ICommandFactory commandFactory,
        ICommandHandler commandHandler)
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        [Function("DiscordCommands")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var sw = Stopwatch.StartNew();
            string body = string.Empty;

            try
            {
                using var buffer = new MemoryStream();
                await req.Body.CopyToAsync(buffer);
                buffer.Position = 0;

                using var reader = new StreamReader(buffer);
                body = await reader.ReadToEndAsync();

                logger.LogInformation("Incoming Discord request:\n{Body}", body);
                buffer.Position = 0; 


                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

                if (!isDevelopment && !auth.VerifyRequest(body, req.Headers, logger))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteAsJsonAsync(new { error = "Invalid signature" });
                    logger.LogWarning("Unauthorized request");
                    return unauthorized;
                }
                    
                var interaction = await JsonSerializer.DeserializeAsync<DiscordInteraction>(
                    buffer,
                    _jsonSerializerOptions
                );

                if (interaction?.Data?.Name is null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteAsJsonAsync(new DiscordResponse(
                        Type: 4,
                        Data: new DiscordResponseData(Content: "No command found.")
                    ));
                    logger.LogWarning("Invalid command payload");
                    return badRequest;
                }
                    
                var command = await commandFactory.Create(interaction.Data.Name);
                var discordResponse = await commandHandler.HandleCommandAsync(command, interaction, _jsonSerializerOptions);

                var jsonResponse = JsonSerializer.Serialize(discordResponse, _jsonSerializerOptions);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(jsonResponse);

                logger.LogInformation("Received command: {Command}", interaction.Data.Name);
                logger.LogInformation("Outgoing Discord response:\n{Response}", jsonResponse);
                logger.LogInformation("Request finished in {Elapsed} ms", sw.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error after {Elapsed} ms. Body: {Body}", sw.ElapsedMilliseconds, body);

                var error = req.CreateResponse(HttpStatusCode.BadRequest);
                error.Headers.Add("Content-Type", "application/json");
                await error.WriteAsJsonAsync(new { error = "Error processing command" });
                return error;
            }
        }
    }
}
