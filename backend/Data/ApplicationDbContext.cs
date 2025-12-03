using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Data;

/// <summary>
/// Unified application database context for monolithic backend
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // User service entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<OAuthProvider> OAuthProviders { get; set; } = null!;
    public DbSet<UserAnalytics> UserAnalytics { get; set; } = null!;

    // Payment service entities
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; } = null!;

    // Menu service entities
    public DbSet<MenuCategory> MenuCategories { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    
    // Order service entities
    public DbSet<Order> Orders { get; set; }

    // Loyalty service entities
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; } = null!;
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; } = null!;
    public DbSet<LoyaltyRedemption> LoyaltyRedemptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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

