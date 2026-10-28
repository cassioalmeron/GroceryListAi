using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryListAi.Core.Configurations;

public class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.ToTable("List");

        builder.HasKey(list => list.Id);

        builder.Property(list => list.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(list => list.Name)
            .IsUnique();

        builder.Property(list => list.CreatedAt)
            .IsRequired();

        builder.Property(list => list.UpdatedAt)
            .IsRequired();
    }
}
