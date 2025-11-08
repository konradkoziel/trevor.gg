using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using System.ClientModel;
using trevor.Commands.Core;
using trevor.Common;
using trevor.Discord;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddTransient<DiscordClient>(sp =>
    new DiscordClient(
        sp.GetRequiredService<HttpClient>()
    )
);

builder.Services.AddSingleton<ChatClient>(sp =>
{
    var endpoint = Environment.GetEnvironmentVariable("OPENAI_URL");
    var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
    var deployment = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT");

    var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
    return azureClient.GetChatClient(deployment);
});

builder.Services.AddSingleton<ICommandFactory, CommandFactory>();

builder.Services.AddSingleton<IAuthentication, Authentication>();
builder.Build().Run();
