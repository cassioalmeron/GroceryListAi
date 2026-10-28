using GroceryListAi.Core.Exceptions;
using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryListAi.Core.Services;

public class ListService : IListService
{
    private readonly GroceryListAiDbContext _dbContext;

    public ListService(GroceryListAiDbContext dbContext) => _dbContext = dbContext;

    public Task<List<List>> GetAllAsync() =>
        _dbContext.Lists
            .AsNoTracking()
            .OrderBy(list => list.Id)
            .ToListAsync();

    public Task<List?> GetByIdAsync(int id) =>
        _dbContext.Lists
            .AsNoTracking()
            .FirstOrDefaultAsync(list => list.Id == id);

    public async Task<List> CreateAsync(string name)
    {
        var trimmedName = name.Trim();
        await EnsureNameIsAvailableAsync(trimmedName, ignoredListId: null);

        var list = new List { Name = trimmedName };

        _dbContext.Lists.Add(list);
        await _dbContext.SaveChangesAsync();

        return list;
    }

    public async Task<List?> RenameAsync(int id, string name)
    {
        var list = await _dbContext.Lists.FindAsync(id);

        if (list is null)
            return null;

        var trimmedName = name.Trim();
        await EnsureNameIsAvailableAsync(trimmedName, ignoredListId: id);

        list.Name = trimmedName;
        await _dbContext.SaveChangesAsync();

        return list;
    }

    public async Task<ListDeleteResult> DeleteAsync(int id)
    {
        var list = await _dbContext.Lists.FindAsync(id);

        if (list is null)
            return ListDeleteResult.NotFound;

        if (!await _dbContext.Lists.AnyAsync(other => other.Id != id))
            return ListDeleteResult.LastList;

        _dbContext.Lists.Remove(list);
        await _dbContext.SaveChangesAsync();

        return ListDeleteResult.Deleted;
    }

    private async Task EnsureNameIsAvailableAsync(string name, int? ignoredListId)
    {
        var lowerCaseName = name.ToLower();

        var nameInUse = await _dbContext.Lists.AnyAsync(list =>
            list.Id != ignoredListId && list.Name.ToLower() == lowerCaseName);

        if (nameInUse)
            throw new DuplicateListNameException(name);
    }
}
