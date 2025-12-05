using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace backend.Data.Configurations;

/// <summary>
/// Order entity configuration
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(o => o.ExternalUserRef)
            .HasColumnName("external_user_ref")
            .HasMaxLength(255);
        
        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(o => o.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(o => o.PaymentStatus)
            .HasColumnName("payment_status")
            .IsRequired();
        
        builder.Property(o => o.OrderStatus)
            .HasColumnName("order_status")
            .IsRequired();
        
        builder.Property(o => o.PlacedAt)
            .HasColumnName("placed_at")
            .IsRequired();
        
        builder.Property(o => o.CancelRequestedAt)
            .HasColumnName("cancel_requested_at");
        
        builder.Property(o => o.CanceledAt)
            .HasColumnName("canceled_at");
        
        builder.Property(o => o.Metadata)
			.HasColumnName("metadata")
            .HasColumnType("jsonb");
        
        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        // Relationships
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

