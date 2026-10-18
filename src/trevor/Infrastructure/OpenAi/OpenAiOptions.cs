namespace trevor.Infrastructure.OpenAi;

// Azure OpenAI, env vars OPENAI_URL, OPENAI_KEY, OPENAI_DEPLOYMENT
public class OpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;

    public bool IsValid =>
        Uri.TryCreate(Endpoint, UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(Key)
        && !string.IsNullOrWhiteSpace(Deployment);
}
