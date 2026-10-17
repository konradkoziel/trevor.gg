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
using trevor.Features.Lol.Common;
using trevor.Features.Lol.Rank;
using trevor.Features.Lol.Roast;
using trevor.Features.Ping;
using trevor.Features.Teams;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

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

builder.Services.AddOptions<RiotOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.ApiKey = config["RIOT_API_KEY"];
        var platform = config["RIOT_PLATFORM"];
        if (!string.IsNullOrWhiteSpace(platform))
            options.Platform = platform.Trim();
    })
    .Validate(o => RiotRouting.IsKnownPlatform(o.Platform), "RIOT_PLATFORM must be a Riot platform id, e.g. eun1 or euw1")
    .ValidateOnStart();

// Infrastructure
builder.Services.AddSingleton(sp =>
{
    var openAi = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    var azureClient = new AzureOpenAIClient(new Uri(openAi.Endpoint), new ApiKeyCredential(openAi.Key));
    return azureClient.GetChatClient(openAi.Deployment);
});
builder.Services.AddSingleton<ITrevorAi, TrevorAi>();
builder.Services.AddSingleton<IRiotPlayerRunner, RiotPlayerRunner>();
builder.Services.AddSingleton<IDiscordSignatureVerifier, DiscordSignatureVerifier>();
builder.Services.AddHttpClient<DiscordApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10));
// Commands are singletons, so this client lives for the whole app: rotate connections
// in the handler instead of relying on IHttpClientFactory's handler recycling
builder.Services.AddHttpClient<IRiotApiClient, RiotApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
    .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
    .AddStandardResilienceHandler(); // retries 429/5xx with back-off, per-attempt and total timeouts

// Commands: register a new slash command here (and in the Discord Developer Portal)
builder.Services.AddSingleton<ICommand, PingCommand>();
builder.Services.AddSingleton<ICommand, TeamsCommand>();
builder.Services.AddSingleton<ICommand, AskCommand>();
builder.Services.AddSingleton<ICommand, RankCommand>();
builder.Services.AddSingleton<ICommand, RoastCommand>();
builder.Services.AddSingleton<CommandRegistry>();
builder.Services.AddSingleton<InteractionHandler>();

builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Build().Run();
