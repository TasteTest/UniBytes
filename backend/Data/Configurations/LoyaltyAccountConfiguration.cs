using backend.Modelss;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

/// <summary>
/// Entity configuration for LoyaltyAccount
/// </summary>
public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("loyalty_accounts");

        builder.HasKey(la => la.Id);

        builder.Property(la => la.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(la => la.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(la => la.UserId)
            .IsUnique()
            .HasDatabaseName("idx_loyalty_accounts_user_id");

        builder.Property(la => la.PointsBalance)
            .HasColumnName("points_balance")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(la => la.Tier)
            .HasColumnName("tier")
            .IsRequired();

        builder.Property(la => la.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(la => la.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(la => la.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(la => la.CreatedAt)
            .HasDatabaseName("idx_loyalty_accounts_created_at");

        builder.HasIndex(la => la.IsActive)
            .HasDatabaseName("idx_loyalty_accounts_is_active");

        builder.HasIndex(la => la.Tier)
            .HasDatabaseName("idx_loyalty_accounts_tier");

        // Relationships
        builder.HasMany(la => la.LoyaltyTransactions)
            .WithOne(lt => lt.LoyaltyAccount)
            .HasForeignKey(lt => lt.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(la => la.LoyaltyRedemptions)
            .WithOne(lr => lr.LoyaltyAccount)
            .HasForeignKey(lr => lr.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
