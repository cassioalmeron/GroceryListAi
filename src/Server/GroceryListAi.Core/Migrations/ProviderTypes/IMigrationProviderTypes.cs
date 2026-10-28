using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace GroceryListAi.Core.Migrations.ProviderTypes;

public interface IMigrationProviderTypes
{
    string Bool { get; }
    string DateTime { get; }
    string BoolLiteral(bool value);
    OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column);
    string? ResetIdentitySequenceSql(string tableName, string idColumn);
    string? RenamePrimaryKeySql(string tableName, string oldName, string newName);
    string? ConvertDateTimeColumnToUtcSql(string tableName, string column);
}
