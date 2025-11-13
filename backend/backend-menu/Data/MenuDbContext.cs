// using backend_menu.Model;
// using Microsoft.EntityFrameworkCore;
// using System.Reflection;

// namespace backend_menu.Data;

// public class MenuDbContext : DbContext
// {
//     public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }

//     public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
//     public DbSet<MenuItem> MenuItems => Set<MenuItem>();

//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.Entity<MenuCategory>(entity =>
//         {
//             entity.ToTable("menu_categories");
//             entity.HasKey(e => e.Id);
//             entity.Property(e => e.Id).HasColumnName("id");
//             entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
//             entity.Property(e => e.Description).HasColumnName("description");
//             entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
//             entity.Property(e => e.IsActive).HasColumnName("is_active");
//             entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
//             entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
//             entity.HasIndex(e => e.Name).IsUnique();
//         });

//         modelBuilder.Entity<MenuItem>(entity =>
//         {
//             entity.ToTable("menu_items");
//             entity.HasKey(e => e.Id);
//             entity.Property(e => e.Id).HasColumnName("id");
//             entity.Property(e => e.CategoryId).HasColumnName("category_id");
//             entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
//             entity.Property(e => e.Description).HasColumnName("description");
//             entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
//             entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
//             entity.Property(e => e.Available).HasColumnName("available");
//             entity.Property(e => e.Visibility).HasColumnName("visibility");
//             entity.Property(e => e.Components).HasColumnName("components").HasColumnType("jsonb");
//             entity.Property(e => e.ImageUrl).HasColumnName("image_url");
//             entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
//             entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

//             entity.HasOne(e => e.Category)
//                   .WithMany(c => c.MenuItems)
//                   .HasForeignKey(e => e.CategoryId)
//                   .OnDelete(DeleteBehavior.Restrict);
//         });
//         // ApplyConfigurationsFromAssembly automatically loads all IEntityTypeConfiguration
//         // classes from Mappings folder (MenuCategoryMapping, MenuItemMapping)
//         modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());    
//     }
// }
using backend_menu.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace backend_menu.Data;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplyConfigurationsFromAssembly automatically loads all IEntityTypeConfiguration
        // classes from Mappings folder (MenuCategoryMapping, MenuItemMapping)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}