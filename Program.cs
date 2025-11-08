using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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



builder.Services.AddSingleton<IAuthentication, Authentication>();
builder.Build().Run();
