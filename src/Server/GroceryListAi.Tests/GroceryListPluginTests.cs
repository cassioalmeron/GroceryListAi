using GroceryListAi.Core;
using GroceryListAi.Core.Llm;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GroceryListAi.Tests;

public class GroceryListPluginTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GroceryListAiDbContext _dbContext;
    private readonly GroceryListPlugin _plugin;

    public GroceryListPluginTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GroceryListAiDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new GroceryListAiDbContext(options);
        _dbContext.Database.Migrate();

        _plugin = new GroceryListPlugin(_dbContext);
    }

    [Fact]
    public async Task GroceryListPluginTests_Adding_an_item_saves_it_and_returns_a_confirmation_message()
    {
        var result = await _plugin.AddItemAsync("Milk");

        Assert.Contains("Milk", result);
        var item = Assert.Single(_dbContext.Items);
        Assert.Equal("Milk", item.Description);
        Assert.False(item.Checked);
    }

    [Fact]
    public async Task GroceryListPluginTests_Removing_with_an_exact_match_removes_the_item()
    {
        await _plugin.AddItemAsync("Bread");

        var result = await _plugin.RemoveItemAsync("Bread");

        Assert.Contains("Bread", result);
        Assert.Empty(_dbContext.Items);
    }

    [Fact]
    public async Task GroceryListPluginTests_Removing_with_a_case_insensitive_substring_match_removes_the_item()
    {
        await _plugin.AddItemAsync("Whole Milk");

        var result = await _plugin.RemoveItemAsync("milk");

        Assert.Contains("Whole Milk", result);
        Assert.Empty(_dbContext.Items);
    }

    [Fact]
    public async Task GroceryListPluginTests_Removing_with_no_match_returns_a_not_found_message_without_throwing()
    {
        var result = await _plugin.RemoveItemAsync("Eggs");

        Assert.Contains("No item found", result);
    }

    [Fact]
    public async Task GroceryListPluginTests_Removing_with_several_matches_removes_only_the_first_one()
    {
        await _plugin.AddItemAsync("Milk 1L");
        await _plugin.AddItemAsync("Milk 2L");

        await _plugin.RemoveItemAsync("Milk");

        var remaining = Assert.Single(_dbContext.Items);
        Assert.Equal("Milk 2L", remaining.Description);
    }

    [Fact]
    public async Task GroceryListPluginTests_Checking_an_item_marks_it_as_checked()
    {
        await _plugin.AddItemAsync("Butter");

        var result = await _plugin.CheckItemAsync("Butter");

        Assert.Contains("Checked", result);
        Assert.True(_dbContext.Items.Single().Checked);
    }

    [Fact]
    public async Task GroceryListPluginTests_Checking_with_no_match_returns_a_not_found_message()
    {
        var result = await _plugin.CheckItemAsync("Cheese");

        Assert.Contains("No item found", result);
    }

    [Fact]
    public async Task GroceryListPluginTests_Unchecking_an_item_marks_it_as_unchecked()
    {
        await _plugin.AddItemAsync("Yogurt");
        await _plugin.CheckItemAsync("Yogurt");

        var result = await _plugin.UncheckItemAsync("Yogurt");

        Assert.Contains("Unchecked", result);
        Assert.False(_dbContext.Items.Single().Checked);
    }

    [Fact]
    public async Task GroceryListPluginTests_Unchecking_with_no_match_returns_a_not_found_message()
    {
        var result = await _plugin.UncheckItemAsync("Carrots");

        Assert.Contains("No item found", result);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
