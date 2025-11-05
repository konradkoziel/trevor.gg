using Discord.WebSocket;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using trevor.Discord;

namespace trevor
{
    public class TimerFunction
    {
        private readonly ILogger _logger;
        private readonly IDiscordClient _discordClient;
        public TimerFunction(ILoggerFactory loggerFactory, IDiscordClient discordClient)
        {
            _logger = loggerFactory.CreateLogger<TimerFunction>();
            _discordClient = discordClient;
        }

        [Function("TimerFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {

            await _discordClient.SendMessageAsync(848237840792944660, "Witajcie!");

                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
