using GroceryListAi.Core.Models;

namespace GroceryListAi.Api.Dtos;

public record ItemResponse(int Id, string Description, bool Checked, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static ItemResponse FromItem(Item item) =>
        new(item.Id, item.Description, item.Checked, item.CreatedAt, item.UpdatedAt);
}
