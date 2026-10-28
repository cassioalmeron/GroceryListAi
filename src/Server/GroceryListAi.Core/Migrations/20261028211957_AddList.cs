using GroceryListAi.Core.Migrations.ProviderTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryListAi.Core.Migrations;

/// <inheritdoc />
public partial class AddList : Migration
{
    private const string DefaultListName = "General";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

        migrationBuilder.CreateTable(
            name: "List",
            columns: table => new
            {
                Id = providerTypes.AsAutoIncrementPrimaryKey(table.Column<int>(nullable: false)),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: providerTypes.DateTime, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: providerTypes.DateTime, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_List", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_List_Name",
            table: "List",
            column: "Name",
            unique: true);

        var now = DateTime.UtcNow;

        // Explicit column types: without them InsertData reads the SQLite-style Designer model, which fails on Postgres
        migrationBuilder.InsertData(
            table: "List",
            columns: ["Name", "CreatedAt", "UpdatedAt"],
            columnTypes: [providerTypes.String(100), providerTypes.DateTime, providerTypes.DateTime],
            values: [DefaultListName, now, now]);

        // Nullable only so the existing rows can be added before they have a value
        migrationBuilder.AddColumn<int>(
            name: "ListId",
            table: "Item",
            nullable: true);

        migrationBuilder.Sql(
            $"""UPDATE "Item" SET "ListId" = (SELECT "Id" FROM "List" WHERE "Name" = '{DefaultListName}');""");

        migrationBuilder.AlterColumn<int>(
            name: "ListId",
            table: "Item",
            nullable: false,
            oldClrType: typeof(int),
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Item_ListId",
            table: "Item",
            column: "ListId");

        migrationBuilder.AddForeignKey(
            name: "FK_Item_List_ListId",
            table: "Item",
            column: "ListId",
            principalTable: "List",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Item_List_ListId",
            table: "Item");

        migrationBuilder.DropIndex(
            name: "IX_Item_ListId",
            table: "Item");

        migrationBuilder.DropColumn(
            name: "ListId",
            table: "Item");

        migrationBuilder.DropTable(
            name: "List");
    }
}
