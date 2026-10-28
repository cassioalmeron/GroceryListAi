using GroceryListAi.Core.Models;

namespace GroceryListAi.Api.Dtos;

public record ItemResponse(int Id, string Description, bool Checked, int ListId, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static ItemResponse FromItem(Item item) =>
        new(item.Id, item.Description, item.Checked, item.ListId, item.CreatedAt, item.UpdatedAt);
}
