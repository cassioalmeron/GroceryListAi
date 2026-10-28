namespace GroceryListAi.Core.Models;

public class List : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<Item> Items { get; set; } = [];
}
