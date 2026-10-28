using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryListAi.Core.Services;

public class ItemService : IItemService
{
    private readonly GroceryListAiDbContext _dbContext;

    public ItemService(GroceryListAiDbContext dbContext) => _dbContext = dbContext;

    public Task<bool> ListExistsAsync(int listId) =>
        _dbContext.Lists.AnyAsync(list => list.Id == listId);

    public Task<List<Item>> GetAllAsync(int listId) =>
        _dbContext.Items
            .AsNoTracking()
            .Where(item => item.ListId == listId)
            .OrderBy(item => item.Id)
            .ToListAsync();

    public async Task<Item> CreateAsync(int listId, string description)
    {
        var item = new Item { ListId = listId, Description = description };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _dbContext.Items.FindAsync(id);

        if (item is null)
            return false;

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Item?> UpdateCheckedAsync(int id, bool isChecked)
    {
        var item = await _dbContext.Items.FindAsync(id);

        if (item is null)
            return null;

        item.Checked = isChecked;
        await _dbContext.SaveChangesAsync();

        return item;
    }
}
