using Azure.Storage.Blobs;
using Lol.Shared.Riot;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api.Infrastructure.Discord;
using Web.Api.Infrastructure.Sessions;

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
    });
builder.Services.AddHttpClient<IDiscordOAuthClient, DiscordOAuthClient>(client => client.Timeout = TimeSpan.FromSeconds(10));

// Sessions: the signing key lives in the API's own storage account (AzureWebJobsStorage)
builder.Services.AddSingleton(sp => new BlobServiceClient(sp.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]));
builder.Services.AddSingleton<ISigningKeyStore, BlobSigningKeyStore>();
builder.Services.AddSingleton<ISessionTokens, SessionTokens>();

builder.Build().Run();
