using GroceryListAi.Api.Dtos;
using GroceryListAi.Core.Llm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace GroceryListAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatController> _logger;

    public ChatController(Kernel kernel, ILogger<ChatController> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    [HttpPost]
    public async Task ChatAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory(SystemPrompt.Text);
        history.AddUserMessage(request.Message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        try
        {
            await foreach (var chunk in chatCompletionService.GetStreamingChatMessageContentsAsync(
                history, executionSettings, _kernel, cancellationToken))
            {
                if (string.IsNullOrEmpty(chunk.Content))
                    continue;

                await Response.WriteAsync(chunk.Content, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while streaming chat response");

            if (!Response.HasStarted)
                Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
