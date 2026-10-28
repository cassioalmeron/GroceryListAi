using GroceryListAi.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryListAi.Core.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Item");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(item => item.Checked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(item => item.ListId)
            .IsRequired();

        builder.HasOne(item => item.List)
            .WithMany(list => list.Items)
            .HasForeignKey(item => item.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.UpdatedAt)
            .IsRequired();
    }
}
