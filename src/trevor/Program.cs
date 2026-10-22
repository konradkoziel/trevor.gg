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
using Lol.Shared.Riot;
using trevor.Core;
using trevor.Features.Ask;
using trevor.Features.Lol.Builds;
using trevor.Features.Lol.Common;
using trevor.Features.Lol.Rank;
using trevor.Features.Lol.Roast;
using trevor.Features.Lol.Tracking;
using trevor.Features.Ping;
using trevor.Features.RateThis;
using trevor.Features.Teams;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Storage;
using trevor.Infrastructure.Web;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuration: env var names are kept as they are set in Azure
builder.Services.AddOptions<DiscordOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.PublicKey = config["DISCORD_PUBLIC_KEY"] ?? string.Empty;
        options.MatchesWebhookUrl = config["DISCORD_MATCHES_WEBHOOK"];
        options.SkipSignatureValidation = string.Equals(config["DISCORD_SKIP_SIGNATURE"], "true", StringComparison.OrdinalIgnoreCase);
    })
    .Validate(o => o.SkipSignatureValidation || o.HasValidPublicKey, "DISCORD_PUBLIC_KEY must be the 64-char hex public key")
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

builder.Services.AddRiot();

builder.Services.AddOptions<WebApiOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.Url = config["WEB_API_URL"];
        options.Key = config["TREVOR_WEB_KEY"];
        options.SiteUrl = config["WEB_SITE_URL"];
    });

builder.Services.AddOptions<StorageOptions>()
    .Configure<IConfiguration>((options, config) => options.ConnectionString = config["AzureWebJobsStorage"]);

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
// Captured by singleton commands, so the handler lives for the whole app and rotates its connections itself
builder.Services.AddHttpClient<IWebApiClient, WebApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
    .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
builder.Services.AddHttpClient<IDiscordApiClient, DiscordApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<StorageTables>();
builder.Services.AddSingleton<ITrackedPlayerStore, TableTrackedPlayerStore>();
builder.Services.AddSingleton<IReportedMatchStore, TableReportedMatchStore>();

// Commands: register a new slash command here and add its definition to deploy/discord-commands.json
builder.Services.AddSingleton<ICommand, PingCommand>();
builder.Services.AddSingleton<ICommand, TeamsCommand>();
builder.Services.AddSingleton<ICommand, AskCommand>();
builder.Services.AddSingleton<ICommand, RankCommand>();
builder.Services.AddSingleton<ICommand, RoastCommand>();
builder.Services.AddSingleton<ICommand, RateThisCommand>();
builder.Services.AddSingleton<ICommand, TrackCommand>();
builder.Services.AddSingleton<ICommand, UntrackCommand>();
builder.Services.AddSingleton<ICommand, BuildCommand>();
builder.Services.AddSingleton<CommandRegistry>();
builder.Services.AddSingleton<InteractionHandler>();

// Transient, so its typed Discord client isn't captured for the app's lifetime
builder.Services.AddTransient<MatchTracker>();

builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Build().Run();
