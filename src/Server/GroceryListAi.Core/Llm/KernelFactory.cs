using Microsoft.SemanticKernel;

namespace GroceryListAi.Core.Llm;

public static class KernelFactory
{
    public static Kernel Create(LlmSettings settings)
    {
        var builder = Kernel.CreateBuilder();

        switch (settings.LlmType)
        {
            case "chatgpt":
                AddOpenAi(builder, settings);
                break;
            case "ollama":
                AddOllama(builder, settings);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported LLM type '{settings.LlmType}'. Expected 'chatgpt' or 'ollama'.");
        }

        return builder.Build();
    }

    private static void AddOpenAi(IKernelBuilder builder, LlmSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.OpenAiApiKey))
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is required when LLM=chatgpt.");

        builder.AddOpenAIChatCompletion(settings.Model, settings.OpenAiApiKey);
    }

#pragma warning disable SKEXP0070
    private static void AddOllama(IKernelBuilder builder, LlmSettings settings) =>
        builder.AddOllamaChatCompletion(settings.Model, new Uri(settings.OllamaEndpoint));
#pragma warning restore SKEXP0070
}
