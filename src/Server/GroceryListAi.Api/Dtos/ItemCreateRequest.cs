using System.ComponentModel.DataAnnotations;

namespace GroceryListAi.Api.Dtos;

public record ItemCreateRequest
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string Description { get; init; }

    public required int ListId { get; init; }
}
