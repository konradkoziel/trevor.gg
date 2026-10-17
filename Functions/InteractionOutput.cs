using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace trevor.Functions;

// Multi-output of the HTTP function: the HTTP response plus an optional queue message
public class InteractionOutput
{
    [HttpResult]
    public required IActionResult Result { get; init; }

    // Null means nothing is written to the queue
    [QueueOutput(DeferredCommandFunction.QueueName)]
    public string? DeferredInteraction { get; init; }
}
