using System.Net;
using System.Net.Http.Json;
using GroceryListAi.Api.Dtos;
using GroceryListAi.Tests.Infrastructure;
using Microsoft.SemanticKernel;

namespace GroceryListAi.Tests;

public class ChatControllerTests
{
    [Fact]
    public async Task ChatControllerTests_A_plain_text_reply_is_streamed_with_the_event_stream_content_type()
    {
        using var factory = new GroceryListAiWebApplicationFactory(
            FakeChatCompletionService.RespondingWithText("Hello", " there!"));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/chat", new ChatRequest { Message = "Hi" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello there!", body);
    }

    [Fact]
    public async Task ChatControllerTests_An_add_item_tool_call_saves_the_item_and_streams_the_follow_up_text()
    {
        var responder = FakeChatCompletionService.InvokingFunction(
            "add_item",
            new KernelArguments { ["description"] = "Milk" },
            "Added Milk to your list!");

        using var factory = new GroceryListAiWebApplicationFactory(responder);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/chat", new ChatRequest { Message = "add milk" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Added Milk to your list!", body);

        var items = await client.GetFromJsonAsync<List<ItemResponse>>("/api/items");
        var item = Assert.Single(items!);
        Assert.Equal("Milk", item.Description);
    }

    [Fact]
    public async Task ChatControllerTests_A_check_item_tool_call_marks_the_existing_item_as_checked()
    {
        using var factory = new GroceryListAiWebApplicationFactory(
            FakeChatCompletionService.InvokingFunction(
                "check_item", new KernelArguments { ["description"] = "eggs" }, "Checked eggs off your list."));
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Eggs" });

        var response = await client.PostAsJsonAsync("/api/chat", new ChatRequest { Message = "check eggs" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Checked eggs off your list.", body);

        var items = await client.GetFromJsonAsync<List<ItemResponse>>("/api/items");
        Assert.True(items!.Single().Checked);
    }

    [Fact]
    public async Task ChatControllerTests_A_tool_call_for_a_missing_item_streams_the_not_found_narration_without_failing_the_request()
    {
        using var factory = new GroceryListAiWebApplicationFactory(
            FakeChatCompletionService.InvokingFunction(
                "remove_item",
                new KernelArguments { ["description"] = "pineapple" },
                "I couldn't find an item matching 'pineapple' on your list."));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/chat", new ChatRequest { Message = "remove pineapple" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("couldn't find", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ChatControllerTests_A_request_without_the_message_field_returns_bad_request()
    {
        using var factory = new GroceryListAiWebApplicationFactory(FakeChatCompletionService.RespondingWithText("hi"));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/chat", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
