using GroceryListAi.Api.Dtos;
using GroceryListAi.Core.Llm;
using GroceryListAi.Core.Services;
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
    private readonly IGroceryListPluginFactory _pluginFactory;
    private readonly IItemService _itemService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        Kernel kernel,
        IGroceryListPluginFactory pluginFactory,
        IItemService itemService,
        ILogger<ChatController> logger)
    {
        _kernel = kernel;
        _pluginFactory = pluginFactory;
        _itemService = itemService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ChatAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _itemService.ListExistsAsync(request.ListId))
                return NotFound(new { message = "List not found" });

            _kernel.Plugins.AddFromObject(_pluginFactory.Create(request.ListId), "GroceryList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing chat for list {ListId}", request.ListId);
            return StatusCode(500, new { message = "Error processing chat" });
        }

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

        return new EmptyResult();
    }
}
