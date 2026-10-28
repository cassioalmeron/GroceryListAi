using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace GroceryListAi.Core.Migrations.ProviderTypes;

public class SqliteMigrationProviderTypes : IMigrationProviderTypes
{
    public string Bool => "INTEGER";
    public string DateTime => "TEXT";

    public string BoolLiteral(bool value) => value ? "1" : "0";

    public OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column) =>
        column.Annotation("Sqlite:Autoincrement", true);

    public string? ResetIdentitySequenceSql(string tableName, string idColumn) => null;

    // SQLite has no named primary key constraints to rename
    public string? RenamePrimaryKeySql(string tableName, string oldName, string newName) => null;

    // SQLite stores dates as TEXT and has no time zone column type to convert to
    public string? ConvertDateTimeColumnToUtcSql(string tableName, string column) => null;
}
