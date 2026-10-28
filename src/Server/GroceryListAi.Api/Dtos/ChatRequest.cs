using System.ComponentModel.DataAnnotations;

namespace GroceryListAi.Api.Dtos;

public record ChatRequest
{
    [Required]
    public required string Message { get; init; }
}
