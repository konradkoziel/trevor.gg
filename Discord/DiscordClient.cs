using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace trevor.Discord
{
    public class DiscordClient : IDiscordClient
    {
        private readonly HttpClient _http;
        private readonly string _token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        public DiscordClient(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.Add("Authorization", $"Bot {_token}");
        }

        public async Task SendMessageAsync(ulong channelId, string message)
        {
            var payload = new { content = message };
            var url = $"https://discord.com/api/v10/channels/{channelId}/messages";
            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
