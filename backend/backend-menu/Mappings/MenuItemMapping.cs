using backend_menu.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_menu.Mappings;

public class MenuItemMapping : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CategoryId).HasColumnName("category_id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description");
        builder.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(e => e.Available).HasColumnName("available");
        builder.Property(e => e.Visibility).HasColumnName("visibility");
        builder.Property(e => e.Components).HasColumnName("components").HasColumnType("jsonb");
        builder.Property(e => e.ImageUrl).HasColumnName("image_url");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.Category)
               .WithMany(c => c.MenuItems)
               .HasForeignKey(e => e.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}