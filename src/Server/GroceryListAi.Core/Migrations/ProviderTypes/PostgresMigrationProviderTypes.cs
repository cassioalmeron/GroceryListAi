using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GroceryListAi.Core.Migrations.ProviderTypes;

public class PostgresMigrationProviderTypes : IMigrationProviderTypes
{
    public string Bool => "boolean";
    public string DateTime => "timestamp with time zone";

    public string String(int maxLength) => $"character varying({maxLength})";

    public string BoolLiteral(bool value) => value ? "true" : "false";

    public OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column) =>
        column.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

    public string? ResetIdentitySequenceSql(string tableName, string idColumn) => $@"
        SELECT setval(pg_get_serial_sequence('""{tableName}""', '{idColumn}'), COALESCE(MAX(""{idColumn}""), 1)) FROM ""{tableName}"";
    ";

    public string? RenamePrimaryKeySql(string tableName, string oldName, string newName) =>
        $@"ALTER TABLE ""{tableName}"" RENAME CONSTRAINT ""{oldName}"" TO ""{newName}"";";

    // Databases created before DateTime became timestamptz hold UTC values in a timestamp column;
    // newer ones already have the right type, and converting those again would shift the values.
    public string? ConvertDateTimeColumnToUtcSql(string tableName, string column) => $@"
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_name = '{tableName}' AND column_name = '{column}'
                    AND data_type = 'timestamp without time zone')
            THEN
                ALTER TABLE ""{tableName}"" ALTER COLUMN ""{column}"" TYPE timestamp with time zone
                    USING ""{column}"" AT TIME ZONE 'UTC';
            END IF;
        END $$;
    ";
}
