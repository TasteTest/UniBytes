using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace backend.Data.Configurations;

/// <summary>
/// OrderItem entity configuration
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        
        builder.HasKey(oi => oi.Id);
        
        builder.Property(oi => oi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(oi => oi.OrderId)
            .HasColumnName("order_id")
            .IsRequired();
        
        builder.Property(oi => oi.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired(false);
        
        builder.Property(oi => oi.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(oi => oi.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(oi => oi.Quantity)
            .HasColumnName("quantity")
            .IsRequired();
        
        builder.Property(oi => oi.Modifiers)
            .HasColumnName("modifiers")
            .HasColumnType("jsonb");
        
        builder.Property(oi => oi.TotalPrice)
            .HasColumnName("total_price")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(oi => oi.IsReward)
            .HasColumnName("is_reward")
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(oi => oi.RewardId)
            .HasColumnName("reward_id")
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(oi => oi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

