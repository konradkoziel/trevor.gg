using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using System.ClientModel;
using System.Configuration;
using trevor.Commands.Core;
using trevor.Common;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();


builder.Services.AddSingleton<ChatClient>(sp =>
{
    var endpoint = Environment.GetEnvironmentVariable("OPENAI_URL");
    var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
    var deployment = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT");

    if (endpoint != null && key != null && deployment != null)
    {
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
        return azureClient.GetChatClient(deployment);
    }
    throw new Exception("Missing OPENAI_URL or OPENAI_KEY");
});

builder.Services.AddSingleton<ICommandFactory, CommandFactory>();
builder.Services.AddSingleton<IAuthentication, Authentication>();
builder.Services.AddSingleton<ICommandHandler, CommandHandler>();
builder.Services.AddScoped<IAuthentication, Authentication>();
builder.Services.AddHttpClient("discord");

builder.Build().Run();