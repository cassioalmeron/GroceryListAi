using GroceryListAi.Core.Llm;

namespace GroceryListAi.Core.Services;

public class GroceryListPluginFactory : IGroceryListPluginFactory
{
    private readonly GroceryListAiDbContext _dbContext;

    public GroceryListPluginFactory(GroceryListAiDbContext dbContext) => _dbContext = dbContext;

    public GroceryListPlugin Create(int listId) => new(_dbContext, listId);
}
