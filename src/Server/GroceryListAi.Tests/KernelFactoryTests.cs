using GroceryListAi.Core.Llm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GroceryListAi.Tests;

public class KernelFactoryTests
{
    [Fact]
    public void KernelFactoryTests_Chatgpt_with_an_api_key_registers_a_chat_completion_service()
    {
        var settings = new LlmSettings { LlmType = "chatgpt", Model = "gpt-4o-mini", OpenAiApiKey = "test-key" };

        var kernel = KernelFactory.Create(settings);

        Assert.NotNull(kernel.Services.GetRequiredService<IChatCompletionService>());
    }

    [Fact]
    public void KernelFactoryTests_Chatgpt_without_an_api_key_throws_invalid_operation_exception()
    {
        var settings = new LlmSettings { LlmType = "chatgpt", Model = "gpt-4o-mini", OpenAiApiKey = null };

        Assert.Throws<InvalidOperationException>(() => KernelFactory.Create(settings));
    }

    [Fact]
    public void KernelFactoryTests_Ollama_registers_a_chat_completion_service()
    {
        var settings = new LlmSettings { LlmType = "ollama", Model = "llama3.1" };

        var kernel = KernelFactory.Create(settings);

        Assert.NotNull(kernel.Services.GetRequiredService<IChatCompletionService>());
    }

    [Fact]
    public void KernelFactoryTests_An_unsupported_llm_type_throws_invalid_operation_exception()
    {
        var settings = new LlmSettings { LlmType = "gemini", Model = "gemini-pro" };

        Assert.Throws<InvalidOperationException>(() => KernelFactory.Create(settings));
    }
}
