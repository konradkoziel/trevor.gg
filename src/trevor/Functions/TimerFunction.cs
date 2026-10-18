using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using trevor.Infrastructure.Discord;

namespace trevor.Functions;

public class TimerFunction(ILogger<TimerFunction> logger, IDiscordApiClient discord, IOptions<DiscordOptions> options)
{
    [Function("TimerFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, CancellationToken ct)
    {
        var webhookUrl = options.Value.GeneralWebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogWarning("Missing DISCORD_URL_OGÓLNY, skipping timer message");
            return;
        }

        await discord.SendWebhookAsync(webhookUrl, "BBualdo to noob", ct);
        logger.LogInformation("Timer message sent, next run at {Next}", myTimer.ScheduleStatus?.Next);
    }
}
