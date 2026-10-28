using GroceryListAi.Core.Models;

namespace GroceryListAi.Core.Services;

public interface IItemService
{
    Task<bool> ListExistsAsync(int listId);
    Task<List<Item>> GetAllAsync(int listId);
    Task<Item> CreateAsync(int listId, string description);
    Task<bool> DeleteAsync(int id);
    Task<Item?> UpdateCheckedAsync(int id, bool isChecked);
}
