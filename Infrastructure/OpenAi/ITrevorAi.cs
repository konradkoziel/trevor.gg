namespace trevor.Infrastructure.OpenAi;

// One place for every AI call: Trevor's persona, output limits and failure handling.
// Commands pass only their own instruction and data.
public interface ITrevorAi
{
    // Null when the model fails or returns nothing, so callers can fall back to a canned line
    Task<string?> ReplyAsync(string instruction, string input, int maxOutputTokens = 200, CancellationToken ct = default);
}
