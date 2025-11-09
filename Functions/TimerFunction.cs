using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using trevor.Model.Response.cs;

namespace trevor.Functions;

public class TimerFunction(ILogger<TimerFunction> logger, HttpClient httpClient)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    [Function("TimerFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            
        }
        var uri = Environment.GetEnvironmentVariable("DISCORD_URL_OGÓLNY");
        var payload = new { content = "BBualdo to noob" };
        if (uri != null) httpClient.BaseAddress = new Uri(uri);
        var json = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        logger.LogInformation("Outgoing Discord request:\n{Json}", json);
        await httpClient.PostAsync(uri, content);
    }
}