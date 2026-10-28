using GroceryListAi.Core.Models;

namespace GroceryListAi.Api.Dtos;

public record ListResponse(int Id, string Name, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static ListResponse FromList(List list) =>
        new(list.Id, list.Name, list.CreatedAt, list.UpdatedAt);
}
