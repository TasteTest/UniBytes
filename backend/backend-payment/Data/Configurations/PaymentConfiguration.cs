using backend_payment.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_payment.Data.Configurations;

/// <summary>
/// Payment entity configuration
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.OrderId)
            .HasColumnName("order_id");
        
        builder.Property(p => p.UserId)
            .HasColumnName("user_id");
        
        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .IsRequired();
        
        builder.Property(p => p.Provider)
            .HasColumnName("provider")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p => p.ProviderPaymentId)
            .HasColumnName("provider_payment_id")
            .HasMaxLength(255);
        
        builder.Property(p => p.ProviderChargeId)
            .HasColumnName("provider_charge_id")
            .HasMaxLength(255);
        
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p => p.RawProviderResponse)
            .HasColumnName("raw_provider_response")
            .HasColumnType("jsonb");
        
        builder.Property(p => p.FailureMessage)
            .HasColumnName("failure_message");
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("idx_payments_order_id");
        
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("idx_payments_user_id");
    }
}

