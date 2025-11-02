using backend_payment.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_payment.Data.Configurations;

/// <summary>
/// Idempotency key entity configuration
/// </summary>
public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");
        
        builder.HasKey(ik => ik.Id);
        
        builder.Property(ik => ik.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ik => ik.Key)
            .HasColumnName("key")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(ik => ik.UserId)
            .HasColumnName("user_id");
        
        builder.Property(ik => ik.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        builder.Property(ik => ik.ExpiresAt)
            .HasColumnName("expires_at");
        
        // Note: updated_at column doesn't exist in idempotency_keys table
        // Ignore UpdatedAt property from BaseEntity
        builder.Ignore(ik => ik.UpdatedAt);
        
        builder.HasIndex(ik => ik.Key)
            .IsUnique()
            .HasDatabaseName("idempotency_keys_key_key");
    }
}

