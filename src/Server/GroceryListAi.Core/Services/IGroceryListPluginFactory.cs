using GroceryListAi.Core.Llm;

namespace GroceryListAi.Core.Services;

public interface IGroceryListPluginFactory
{
    GroceryListPlugin Create(int listId);
}
