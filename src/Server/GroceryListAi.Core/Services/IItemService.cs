using GroceryListAi.Core.Models;

namespace GroceryListAi.Core.Services;

public interface IItemService
{
    Task<List<Item>> GetAllAsync();
    Task<Item> CreateAsync(string description);
    Task<bool> DeleteAsync(int id);
    Task<Item?> UpdateCheckedAsync(int id, bool isChecked);
}
