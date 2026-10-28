using GroceryListAi.Core.Migrations.ProviderTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryListAi.Core.Migrations;

/// <inheritdoc />
public partial class RenameItemsTableToItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

        migrationBuilder.RenameTable(name: "Items", newName: "Item");

        var renamePrimaryKeySql = providerTypes.RenamePrimaryKeySql("Item", "PK_Items", "PK_Item");

        if (renamePrimaryKeySql is not null)
            migrationBuilder.Sql(renamePrimaryKeySql);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

        var renamePrimaryKeySql = providerTypes.RenamePrimaryKeySql("Item", "PK_Item", "PK_Items");

        if (renamePrimaryKeySql is not null)
            migrationBuilder.Sql(renamePrimaryKeySql);

        migrationBuilder.RenameTable(name: "Item", newName: "Items");
    }
}
