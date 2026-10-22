using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Lol.Shared.Riot;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api.Features.Bot;
using Web.Api.Features.Builds;
using Web.Api.Features.Home;
using Web.Api.Features.Players;
using Web.Api.Features.RiotAccount;
using Web.Api.Features.Stats;
using Web.Api.Infrastructure.Discord;
using Web.Api.Infrastructure.Sessions;
using Web.Api.Infrastructure.Storage;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddRiot();

// Login with Discord: optional, the API runs without it and the login endpoints answer 503
builder.Services.AddOptions<DiscordOAuthOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.ClientId = config["DISCORD_CLIENT_ID"] ?? string.Empty;
        options.ClientSecret = config["DISCORD_CLIENT_SECRET"] ?? string.Empty;
        options.RedirectUri = config["DISCORD_REDIRECT_URI"] ?? string.Empty;
        options.SiteUrl = config["SITE_URL"] ?? string.Empty;
        options.GuildId = config["DISCORD_GUILD_ID"];
    });
builder.Services.AddHttpClient<IDiscordOAuthClient, DiscordOAuthClient>(client => client.Timeout = TimeSpan.FromSeconds(10));

// The API's own storage account (AzureWebJobsStorage): session signing key (blob) and builds (table)
builder.Services.AddSingleton(sp => new BlobServiceClient(sp.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]));
builder.Services.AddSingleton(sp => new TableServiceClient(sp.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]));
builder.Services.AddSingleton<ISigningKeyStore, BlobSigningKeyStore>();
builder.Services.AddSingleton<ISessionTokens, SessionTokens>();

// Linked Riot accounts (Riot API key: RIOT_API_KEY, optional — without it linking answers 503)
builder.Services.AddSingleton<IRiotLinkStore, TableRiotLinkStore>();
builder.Services.AddSingleton(Random.Shared);

// Stats of the linked account; MatchHistory is a singleton so its refresh throttle is shared
builder.Services.AddSingleton<IMatchHistoryStore, TableMatchHistoryStore>();
builder.Services.AddSingleton<MatchHistory>();
builder.Services.AddSingleton<StatsReport>();

// Builds
builder.Services.AddSingleton<IBuildStore, TableBuildStore>();
builder.Services.AddSingleton<BuildValidator>();

// Home page: singletons so their caches are shared
builder.Services.AddSingleton<FreeRotation>();
builder.Services.AddSingleton<Leaderboard>();

// Player profiles (/gracz/...) of linked friends
builder.Services.AddSingleton<PlayerProfiles>();

// Endpoints for the bot (/api/bot/...), behind the shared BOT_API_KEY
builder.Services.AddOptions<BotApiOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.Key = config["BOT_API_KEY"] ?? string.Empty;
        options.SiteUrl = config["SITE_URL"] ?? string.Empty;
    });

builder.Build().Run();
