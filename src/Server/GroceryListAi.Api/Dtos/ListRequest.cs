using System.ComponentModel.DataAnnotations;

namespace GroceryListAi.Api.Dtos;

public record ListRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}
