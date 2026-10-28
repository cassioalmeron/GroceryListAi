using System;
using GroceryListAi.Core.Migrations.ProviderTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryListAi.Core.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

        migrationBuilder.CreateTable(
            name: "Items",
            columns: table => new
            {
                Id = providerTypes.AsAutoIncrementPrimaryKey(table.Column<int>(nullable: false)),
                Description = table.Column<string>(maxLength: 255, nullable: false),
                Checked = table.Column<bool>(type: providerTypes.Bool, nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: providerTypes.DateTime, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: providerTypes.DateTime, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Items", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Items");
    }
}
