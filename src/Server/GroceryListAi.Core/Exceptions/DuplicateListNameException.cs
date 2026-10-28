namespace GroceryListAi.Core.Exceptions;

public class DuplicateListNameException(string name) : Exception($"A list named '{name}' already exists");
