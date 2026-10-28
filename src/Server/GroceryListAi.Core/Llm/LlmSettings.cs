namespace GroceryListAi.Core.Llm;

public record LlmSettings
{
    public required string LlmType { get; init; }
    public required string Model { get; init; }
    public string? OpenAiApiKey { get; init; }
    public string OllamaEndpoint { get; init; } = "http://localhost:11434";

    public static LlmSettings FromEnvironment()
    {
        var llmType = (Environment.GetEnvironmentVariable("LLM") ?? "ollama").ToLowerInvariant();

        var model = Environment.GetEnvironmentVariable("MODEL")
            ?? throw new InvalidOperationException("MODEL environment variable is required.");

        return new LlmSettings
        {
            LlmType = llmType,
            Model = model,
            OpenAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        };
    }
}
