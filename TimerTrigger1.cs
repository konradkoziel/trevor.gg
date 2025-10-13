using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace trevor.Function;

public class TimerTrigger1
{
    private readonly ILogger _logger;

    public TimerTrigger1(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimerTrigger1>();
    }

    [Function("TimerTrigger1")]
    public void Run([TimerTrigger("0 0 16 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}