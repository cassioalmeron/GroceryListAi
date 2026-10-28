using System.Net;
using System.Net.Http.Json;
using GroceryListAi.Api.Dtos;
using GroceryListAi.Tests.Infrastructure;

namespace GroceryListAi.Tests;

public class ItemsControllerTests : IDisposable
{
    private readonly GroceryListAiWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ItemsControllerTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task ItemsControllerTests_Listing_returns_an_empty_list_when_there_are_no_items()
    {
        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Empty(items!);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_returns_the_new_item_with_its_defaults()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Milk", ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(item);
        Assert.Equal(GroceryListAiWebApplicationFactory.GeneralListId, item!.ListId);
        Assert.True(item!.Id > 0);
        Assert.Equal("Milk", item.Description);
        Assert.False(item.Checked);
        Assert.Equal(item.CreatedAt, item.UpdatedAt);
    }

    [Fact]
    public async Task ItemsControllerTests_A_created_item_appears_in_the_list()
    {
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Bread", ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Single(items!);
        Assert.Equal("Bread", items![0].Description);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_with_an_empty_description_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "", ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_with_a_description_over_the_max_length_returns_bad_request()
    {
        var tooLong = new string('a', 256);

        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = tooLong, ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_without_the_description_field_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_with_a_unicode_description_keeps_the_exact_text()
    {
        const string description = "Café ☕ – crème fraîche";

        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = description, ListId = GroceryListAiWebApplicationFactory.GeneralListId });
        var item = await response.Content.ReadFromJsonAsync<ItemResponse>();

        Assert.Equal(description, item!.Description);
    }

    [Fact]
    public async Task ItemsControllerTests_Listing_returns_the_items_in_insertion_order()
    {
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "First", ListId = GroceryListAiWebApplicationFactory.GeneralListId });
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Second", ListId = GroceryListAiWebApplicationFactory.GeneralListId });
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Third", ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Equal(["First", "Second", "Third"], items!.Select(item => item.Description));
    }

    [Fact]
    public async Task ItemsControllerTests_Deleting_removes_the_item_and_returns_the_deleted_message()
    {
        var created = await CreateItemAsync("Eggs");

        var response = await _client.DeleteAsync($"/api/items/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("deleted", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");
        Assert.Empty(items!);
    }

    [Fact]
    public async Task ItemsControllerTests_Deleting_an_unknown_id_returns_not_found_with_the_not_found_message()
    {
        var response = await _client.DeleteAsync("/api/items/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("not found", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ItemsControllerTests_Deleting_with_a_non_integer_id_returns_not_found()
    {
        var response = await _client.DeleteAsync("/api/items/not-a-number");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Setting_checked_to_true_returns_the_updated_item()
    {
        var created = await CreateItemAsync("Butter");

        var response = await _client.PatchAsJsonAsync($"/api/items/{created.Id}/checked", new ItemCheckedUpdateRequest { Checked = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.True(updated!.Checked);
    }

    [Fact]
    public async Task ItemsControllerTests_Checked_can_be_toggled_back_to_false()
    {
        var created = await CreateItemAsync("Cheese");
        await _client.PatchAsJsonAsync($"/api/items/{created.Id}/checked", new ItemCheckedUpdateRequest { Checked = true });

        var response = await _client.PatchAsJsonAsync($"/api/items/{created.Id}/checked", new ItemCheckedUpdateRequest { Checked = false });
        var updated = await response.Content.ReadFromJsonAsync<ItemResponse>();

        Assert.False(updated!.Checked);
    }

    [Fact]
    public async Task ItemsControllerTests_Updating_checked_refreshes_the_updated_at_timestamp()
    {
        var created = await CreateItemAsync("Yogurt");
        // The system clock can tick coarsely; make sure "now" has moved past the creation time
        SpinWait.SpinUntil(() => DateTime.UtcNow > created.UpdatedAt);

        var response = await _client.PatchAsJsonAsync($"/api/items/{created.Id}/checked", new ItemCheckedUpdateRequest { Checked = true });
        var updated = await response.Content.ReadFromJsonAsync<ItemResponse>();

        Assert.True(updated!.UpdatedAt > created.UpdatedAt);
    }

    [Fact]
    public async Task ItemsControllerTests_Updating_checked_for_an_unknown_id_returns_not_found()
    {
        var response = await _client.PatchAsJsonAsync("/api/items/999999/checked", new ItemCheckedUpdateRequest { Checked = true });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_After_adding_several_items_and_deleting_one_the_remaining_list_is_correct()
    {
        var first = await CreateItemAsync("Apples");
        var second = await CreateItemAsync("Bananas");
        await CreateItemAsync("Carrots");

        await _client.DeleteAsync($"/api/items/{second.Id}");

        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Equal(2, items!.Count);
        Assert.Contains(items, item => item.Id == first.Id);
        Assert.DoesNotContain(items, item => item.Id == second.Id);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_an_item_does_not_affect_another_tests_database()
    {
        // Each test class instance gets its own in-memory SQLite connection (see
        // GroceryListAiWebApplicationFactory), so this should always start empty.
        var items = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Empty(items!);
    }

    [Fact]
    public async Task ItemsControllerTests_Listing_without_the_list_id_returns_bad_request()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Listing_an_unknown_list_returns_not_found()
    {
        var response = await _client.GetAsync("/api/items?listId=999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_in_an_unknown_list_returns_not_found()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Milk", ListId = 999999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Creating_without_the_list_id_field_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/items", new { Description = "Milk" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ItemsControllerTests_Listing_returns_only_the_items_of_the_requested_list()
    {
        var pharmacyId = await _factory.CreateListAsync("Pharmacy");
        await CreateItemAsync("Milk");
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Aspirin", ListId = pharmacyId });

        var general = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");
        var pharmacy = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={pharmacyId}");

        Assert.Equal("Milk", Assert.Single(general!).Description);
        var aspirin = Assert.Single(pharmacy!);
        Assert.Equal("Aspirin", aspirin.Description);
        Assert.Equal(pharmacyId, aspirin.ListId);
    }

    public void Dispose() => _factory.Dispose();

    private async Task<ItemResponse> CreateItemAsync(string description)
    {
        var response = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = description, ListId = GroceryListAiWebApplicationFactory.GeneralListId });
        return (await response.Content.ReadFromJsonAsync<ItemResponse>())!;
    }
}
