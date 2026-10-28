using System.ComponentModel;
using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace GroceryListAi.Core.Llm;

public class GroceryListPlugin
{
    private readonly GroceryListAiDbContext _dbContext;

    public GroceryListPlugin(GroceryListAiDbContext dbContext) => _dbContext = dbContext;

    [KernelFunction("add_item")]
    [Description("Adds a new item to the grocery list.")]
    public async Task<string> AddItemAsync([Description("The item description, e.g. 'Milk'.")] string description)
    {
        var item = new Item { Description = description };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        return $"Added '{description}' to the list.";
    }

    [KernelFunction("remove_item")]
    [Description("Removes an item from the grocery list.")]
    public async Task<string> RemoveItemAsync(
        [Description("Text to search for in the item description, case-insensitive.")] string description)
    {
        var item = await FindItemAsync(description);

        if (item is null)
            return $"No item found matching '{description}'.";

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync();

        return $"Removed '{item.Description}' from the list.";
    }

    [KernelFunction("check_item")]
    [Description("Marks an item on the grocery list as checked/completed.")]
    public async Task<string> CheckItemAsync(
        [Description("Text to search for in the item description, case-insensitive.")] string description)
    {
        var item = await FindItemAsync(description);

        if (item is null)
            return $"No item found matching '{description}'.";

        item.Checked = true;
        await _dbContext.SaveChangesAsync();

        return $"Checked '{item.Description}'.";
    }

    [KernelFunction("uncheck_item")]
    [Description("Marks an item on the grocery list as unchecked/not completed.")]
    public async Task<string> UncheckItemAsync(
        [Description("Text to search for in the item description, case-insensitive.")] string description)
    {
        var item = await FindItemAsync(description);

        if (item is null)
            return $"No item found matching '{description}'.";

        item.Checked = false;
        await _dbContext.SaveChangesAsync();

        return $"Unchecked '{item.Description}'.";
    }

    private async Task<Item?> FindItemAsync(string description)
    {
        var normalized = description.ToLower();

        return await _dbContext.Items
            .Where(item => item.Description.ToLower().Contains(normalized))
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync();
    }
}
