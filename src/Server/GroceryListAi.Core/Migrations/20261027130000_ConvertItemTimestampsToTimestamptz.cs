using GroceryListAi.Core.Migrations.ProviderTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryListAi.Core.Migrations;

/// <inheritdoc />
public partial class ConvertItemTimestampsToTimestamptz : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

        foreach (var column in new[] { "CreatedAt", "UpdatedAt" })
        {
            var convertSql = providerTypes.ConvertDateTimeColumnToUtcSql("Item", column);

            if (convertSql is not null)
                migrationBuilder.Sql(convertSql);
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
