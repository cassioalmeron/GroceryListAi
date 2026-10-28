using System.Net;
using System.Net.Http.Json;
using GroceryListAi.Api.Dtos;
using GroceryListAi.Tests.Infrastructure;

namespace GroceryListAi.Tests;

public class ListsControllerTests : IDisposable
{
    private readonly GroceryListAiWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ListsControllerTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task ListsControllerTests_A_fresh_database_has_exactly_one_list_named_General()
    {
        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");

        var general = Assert.Single(lists!);
        Assert.Equal("General", general.Name);
        Assert.Equal(GroceryListAiWebApplicationFactory.GeneralListId, general.Id);
    }

    [Fact]
    public async Task ListsControllerTests_Creating_returns_the_new_list_and_it_appears_in_the_listing()
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = "Pharmacy" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ListResponse>();
        Assert.Equal("Pharmacy", created!.Name);
        Assert.True(created.Id > 0);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");
        Assert.Equal(["General", "Pharmacy"], lists!.Select(list => list.Name));
    }

    [Fact]
    public async Task ListsControllerTests_Creating_trims_the_name()
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = "  Party  " });
        var created = await response.Content.ReadFromJsonAsync<ListResponse>();

        Assert.Equal("Party", created!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListsControllerTests_Creating_with_a_blank_name_returns_bad_request(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Creating_with_a_name_over_the_max_length_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = new string('a', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Creating_with_a_name_of_the_max_length_succeeds()
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = new string('a', 100) });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Creating_a_duplicate_name_ignoring_case_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = "  gENERAL " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("already exists", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");
        Assert.Single(lists!);
    }

    [Fact]
    public async Task ListsControllerTests_Renaming_returns_the_list_with_the_new_name()
    {
        var created = await CreateListAsync("Pharmacy");

        var response = await _client.PutAsJsonAsync($"/api/lists/{created.Id}", new ListRequest { Name = " Drugstore " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var renamed = await response.Content.ReadFromJsonAsync<ListResponse>();
        Assert.Equal(created.Id, renamed!.Id);
        Assert.Equal("Drugstore", renamed.Name);

        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");
        Assert.Contains(lists!, list => list.Id == created.Id && list.Name == "Drugstore");
    }

    [Fact]
    public async Task ListsControllerTests_Renaming_a_list_to_a_different_casing_of_its_own_name_succeeds()
    {
        var created = await CreateListAsync("Pharmacy");

        var response = await _client.PutAsJsonAsync($"/api/lists/{created.Id}", new ListRequest { Name = "PHARMACY" });
        var renamed = await response.Content.ReadFromJsonAsync<ListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("PHARMACY", renamed!.Name);
    }

    [Fact]
    public async Task ListsControllerTests_Renaming_to_the_name_of_another_list_ignoring_case_returns_bad_request()
    {
        var created = await CreateListAsync("Pharmacy");

        var response = await _client.PutAsJsonAsync($"/api/lists/{created.Id}", new ListRequest { Name = "general" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Renaming_an_unknown_list_returns_not_found()
    {
        var response = await _client.PutAsJsonAsync("/api/lists/999999", new ListRequest { Name = "Anything" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Renaming_with_a_blank_name_returns_bad_request()
    {
        var response = await _client.PutAsJsonAsync($"/api/lists/{GroceryListAiWebApplicationFactory.GeneralListId}", new ListRequest { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListsControllerTests_Deleting_a_list_also_deletes_its_items()
    {
        var pharmacy = await CreateListAsync("Pharmacy");
        var itemResponse = await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Aspirin", ListId = pharmacy.Id });
        var item = await itemResponse.Content.ReadFromJsonAsync<ItemResponse>();
        await _client.PostAsJsonAsync("/api/items", new ItemCreateRequest { Description = "Milk", ListId = GroceryListAiWebApplicationFactory.GeneralListId });

        var response = await _client.DeleteAsync($"/api/lists/{pharmacy.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("deleted", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");
        Assert.Equal("General", Assert.Single(lists!).Name);

        var deleteItemResponse = await _client.DeleteAsync($"/api/items/{item!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteItemResponse.StatusCode);

        var remaining = await _client.GetFromJsonAsync<List<ItemResponse>>($"/api/items?listId={GroceryListAiWebApplicationFactory.GeneralListId}");
        Assert.Equal("Milk", Assert.Single(remaining!).Description);
    }

    [Fact]
    public async Task ListsControllerTests_Deleting_the_last_list_returns_bad_request_and_keeps_it()
    {
        var response = await _client.DeleteAsync($"/api/lists/{GroceryListAiWebApplicationFactory.GeneralListId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("last list", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var lists = await _client.GetFromJsonAsync<List<ListResponse>>("/api/lists");
        Assert.Single(lists!);
    }

    [Fact]
    public async Task ListsControllerTests_Deleting_an_unknown_list_returns_not_found()
    {
        var response = await _client.DeleteAsync("/api/lists/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose() => _factory.Dispose();

    private async Task<ListResponse> CreateListAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/lists", new ListRequest { Name = name });
        return (await response.Content.ReadFromJsonAsync<ListResponse>())!;
    }
}
