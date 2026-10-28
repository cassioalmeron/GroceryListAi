using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GroceryListAi.Tests.Infrastructure;

public class FakeChatCompletionService : IChatCompletionService
{
    private readonly Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>> _respond;

    public FakeChatCompletionService(Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>> respond) => _respond = respond;

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This fake only supports streaming.");

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default) =>
        _respond(kernel);

    /// <summary>Simulates a model turn that replies with plain text only, no tool calls.</summary>
    public static Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>> RespondingWithText(
        params string[] chunks) =>
        _ => StreamText(chunks);

    /// <summary>
    /// Simulates a model turn that invokes a <see cref="GroceryListAi.Core.Llm.GroceryListPlugin"/> function
    /// (as a real auto-invoking connector would) and then streams a follow-up text reply.
    /// </summary>
    public static Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>> InvokingFunction(
        string functionName, KernelArguments arguments, params string[] followUpChunks) =>
        kernel => InvokeThenRespond(kernel!, functionName, arguments, followUpChunks);

    private static async IAsyncEnumerable<StreamingChatMessageContent> StreamText(string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, chunk);
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<StreamingChatMessageContent> InvokeThenRespond(
        Kernel kernel, string functionName, KernelArguments arguments, string[] followUpChunks)
    {
        await kernel.InvokeAsync("GroceryList", functionName, arguments);

        foreach (var chunk in followUpChunks)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, chunk);
            await Task.Yield();
        }
    }
}
