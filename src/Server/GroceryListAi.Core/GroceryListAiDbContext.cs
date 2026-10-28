using System.Reflection;
using GroceryListAi.Core.Migrations.ProviderTypes;
using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GroceryListAi.Core;

public class GroceryListAiDbContext : DbContext
{
    public GroceryListAiDbContext(DbContextOptions<GroceryListAiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Postgres only accepts DateTime values with Kind=Utc
        if (Database.ProviderName == MigrationProviderTypes.Postgres)
            ApplyUtcDateTimeConverter(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        ConvertDateTimesToUtc();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        ConvertDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyUtcDateTimeConverter(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            value => value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        var dateTimeProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?));

        foreach (var property in dateTimeProperties)
            property.SetValueConverter(utcConverter);
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Item>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }

    private void ConvertDateTimesToUtc()
    {
        var dateTimeProperties = ChangeTracker.Entries()
            .SelectMany(entry => entry.Properties)
            .Where(property => property.CurrentValue is DateTime);

        foreach (var property in dateTimeProperties)
        {
            var dateTime = (DateTime)property.CurrentValue!;

            if (dateTime.Kind == DateTimeKind.Unspecified)
                property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            else if (dateTime.Kind == DateTimeKind.Local)
                property.CurrentValue = dateTime.ToUniversalTime();
        }
    }
}
