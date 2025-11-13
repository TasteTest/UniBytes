using backend_menu.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend_menu.Data;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options)
    {
    }

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuDbContext).Assembly);

        // Special handling for InMemory database (used in tests)
        // InMemory doesn't support JsonDocument natively like PostgreSQL does
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Components)
                .HasConversion(
                    v => v != null ? v.RootElement.GetRawText() : "[]",
                    v => JsonDocument.Parse(v ?? "[]", default(JsonDocumentOptions))
                );
        }
    }
}