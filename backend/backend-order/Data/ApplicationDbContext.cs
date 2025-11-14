using Microsoft.EntityFrameworkCore;
using DefaultNamespace;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.unit_price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.total_price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.modifiers).HasColumnType("jsonb");
        });
    }
}