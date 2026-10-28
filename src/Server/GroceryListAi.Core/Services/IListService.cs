using GroceryListAi.Core.Models;

namespace GroceryListAi.Core.Services;

public interface IListService
{
    Task<List<List>> GetAllAsync();
    Task<List?> GetByIdAsync(int id);
    Task<List> CreateAsync(string name);
    Task<List?> RenameAsync(int id, string name);
    Task<ListDeleteResult> DeleteAsync(int id);
}
