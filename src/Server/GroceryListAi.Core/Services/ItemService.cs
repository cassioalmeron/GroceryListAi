using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryListAi.Core.Services;

public class ItemService : IItemService
{
    private readonly GroceryListAiDbContext _dbContext;

    public ItemService(GroceryListAiDbContext dbContext) => _dbContext = dbContext;

    public Task<List<Item>> GetAllAsync() =>
        _dbContext.Items
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToListAsync();

    public async Task<Item> CreateAsync(string description)
    {
        var item = new Item { Description = description };

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
