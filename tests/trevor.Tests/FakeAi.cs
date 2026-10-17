using trevor.Infrastructure.OpenAi;

namespace trevor.Tests;

public sealed class FakeAi(string? reply = "AI says hi") : ITrevorAi
{
    public string? LastInstruction { get; private set; }
    public string? LastInput { get; private set; }
    public int Calls { get; private set; }

    public Task<string?> ReplyAsync(string instruction, string input, int maxOutputTokens = 200, CancellationToken ct = default)
    {
        Calls++;
        LastInstruction = instruction;
        LastInput = input;
        return Task.FromResult(reply);
    }
}
