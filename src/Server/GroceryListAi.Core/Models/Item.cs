namespace GroceryListAi.Core.Models;

public class Item : AuditableEntity
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public bool Checked { get; set; }
    public int ListId { get; set; }

    public List? List { get; set; }
}
