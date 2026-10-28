namespace GroceryListAi.Api.Dtos;

public record ItemCheckedUpdateRequest
{
    public required bool Checked { get; init; }
}
