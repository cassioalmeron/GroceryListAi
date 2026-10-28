using GroceryListAi.Core;
using GroceryListAi.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GroceryListAi.Tests;

public class MigrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GroceryListAiDbContext _dbContext;

    public MigrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GroceryListAiDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new GroceryListAiDbContext(options);
    }

    [Fact]
    public async Task MigrationTests_Add_list_moves_every_existing_item_into_the_general_list()
    {
        var migrator = _dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync("ConvertItemTimestampsToTimestamptz");
        await _dbContext.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Item" ("Description", "Checked", "CreatedAt", "UpdatedAt")
            VALUES ('Milk', 0, '2026-01-01 10:00:00', '2026-01-01 10:00:00'),
                   ('Bread', 1, '2026-01-02 10:00:00', '2026-01-02 10:00:00'),
                   ('Eggs', 0, '2026-01-03 10:00:00', '2026-01-03 10:00:00')
            """);

        await migrator.MigrateAsync("AddList");

        var generalListId = await _dbContext.Lists
            .Where(list => list.Name == "General")
            .Select(list => list.Id)
            .SingleAsync();
        var items = await _dbContext.Items.OrderBy(item => item.Id).ToListAsync();
        Assert.Equal(["Milk", "Bread", "Eggs"], items.Select(item => item.Description));
        Assert.All(items, item => Assert.Equal(generalListId, item.ListId));
        Assert.Equal(1, await _dbContext.Lists.CountAsync());
    }

    [Fact]
    public async Task MigrationTests_After_add_list_inserting_an_item_without_a_list_id_fails_with_a_not_null_constraint_error()
    {
        await _dbContext.Database.MigrateAsync();

        var exception = await Assert.ThrowsAsync<SqliteException>(() => _dbContext.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Item" ("Description", "Checked", "ListId", "CreatedAt", "UpdatedAt")
            VALUES ('Milk', 0, NULL, '2026-01-01 10:00:00', '2026-01-01 10:00:00')
            """));

        Assert.Equal(19, exception.SqliteErrorCode);
        Assert.Contains("NOT NULL", exception.Message);
    }

    [Fact]
    public async Task MigrationTests_After_add_list_inserting_an_item_for_a_missing_list_fails_with_a_foreign_key_error()
    {
        await _dbContext.Database.MigrateAsync();

        var exception = await Assert.ThrowsAsync<SqliteException>(() => _dbContext.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Item" ("Description", "Checked", "ListId", "CreatedAt", "UpdatedAt")
            VALUES ('Milk', 0, 999, '2026-01-01 10:00:00', '2026-01-01 10:00:00')
            """));

        Assert.Contains("FOREIGN KEY", exception.Message);
    }

    [Fact]
    public async Task MigrationTests_Deleting_a_list_deletes_its_items()
    {
        await _dbContext.Database.MigrateAsync();
        var list = new List { Name = "Pharmacy" };
        list.Items.Add(new Item { Description = "Aspirin" });
        _dbContext.Lists.Add(list);
        await _dbContext.SaveChangesAsync();

        _dbContext.Lists.Remove(list);
        await _dbContext.SaveChangesAsync();

        Assert.Empty(await _dbContext.Items.ToListAsync());
    }

    [Fact]
    public void MigrationTests_Every_migration_generates_a_postgres_script_that_seeds_the_general_list()
    {
        // Script generation needs no running server, so this catches Postgres-only SQL generation errors
        var options = new DbContextOptionsBuilder<GroceryListAiDbContext>()
            .UseNpgsql("Host=localhost")
            .Options;
        using var postgresContext = new GroceryListAiDbContext(options);

        var script = postgresContext.GetService<IMigrator>().GenerateScript();

        Assert.Contains("""INSERT INTO "List" """, script);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
