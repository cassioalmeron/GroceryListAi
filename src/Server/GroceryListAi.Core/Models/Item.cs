namespace GroceryListAi.Core.Models;

public record Item
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public bool Checked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
