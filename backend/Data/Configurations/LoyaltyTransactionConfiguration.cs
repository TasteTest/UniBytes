using backend.Modelss;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

/// <summary>
/// Entity configuration for LoyaltyTransaction
/// </summary>
public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.ToTable("loyalty_transactions");

        builder.HasKey(lt => lt.Id);

        builder.Property(lt => lt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(lt => lt.LoyaltyAccountId)
            .HasColumnName("loyalty_account_id")
            .IsRequired();

        builder.Property(lt => lt.ChangeAmount)
            .HasColumnName("change_amount")
            .IsRequired();

        builder.Property(lt => lt.Reason)
            .HasColumnName("reason")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(lt => lt.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(lt => lt.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(lt => lt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(lt => lt.LoyaltyAccountId)
            .HasDatabaseName("idx_loyalty_transactions_account_id");

        builder.HasIndex(lt => lt.ReferenceId)
            .HasDatabaseName("idx_loyalty_transactions_reference_id");

        builder.HasIndex(lt => lt.CreatedAt)
            .HasDatabaseName("idx_loyalty_transactions_created_at");

        // Foreign key relationship
        builder.HasOne(lt => lt.LoyaltyAccount)
            .WithMany(la => la.LoyaltyTransactions)
            .HasForeignKey(lt => lt.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
