using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;
using trevor.Core;
using trevor.Features.Ask;
using trevor.Features.Ping;
using trevor.Features.Teams;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.OpenAi;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuration: env var names are kept as they are set in Azure
builder.Services.AddOptions<DiscordOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.PublicKey = config["DISCORD_API_KEY"] ?? string.Empty;
        options.GeneralWebhookUrl = config["DISCORD_URL_OGÓLNY"];
        options.SkipSignatureValidation = string.Equals(config["DISCORD_SKIP_SIGNATURE"], "true", StringComparison.OrdinalIgnoreCase);
    })
    .Validate(o => o.SkipSignatureValidation || o.HasValidPublicKey, "DISCORD_API_KEY must be the 64-char hex public key")
    .ValidateOnStart();

builder.Services.AddOptions<OpenAiOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.Endpoint = config["OPENAI_URL"] ?? string.Empty;
        options.Key = config["OPENAI_KEY"] ?? string.Empty;
        options.Deployment = config["OPENAI_DEPLOYMENT"] ?? string.Empty;
    })
    .Validate(o => o.IsValid, "OPENAI_URL, OPENAI_KEY and OPENAI_DEPLOYMENT must be set")
    .ValidateOnStart();

// Infrastructure
builder.Services.AddSingleton(sp =>
{
    var openAi = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    var azureClient = new AzureOpenAIClient(new Uri(openAi.Endpoint), new ApiKeyCredential(openAi.Key));
    return azureClient.GetChatClient(openAi.Deployment);
});
builder.Services.AddSingleton<IDiscordSignatureVerifier, DiscordSignatureVerifier>();
builder.Services.AddHttpClient<DiscordApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10));

// Commands: register a new slash command here (and in the Discord Developer Portal)
builder.Services.AddSingleton<ICommand, PingCommand>();
builder.Services.AddSingleton<ICommand, TeamsCommand>();
builder.Services.AddSingleton<ICommand, AskCommand>();
builder.Services.AddSingleton<CommandRegistry>();
builder.Services.AddSingleton<InteractionHandler>();

builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Build().Run();
