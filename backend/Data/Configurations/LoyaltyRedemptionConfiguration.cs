using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

/// <summary>
/// Entity configuration for LoyaltyRedemption
/// </summary>
public class LoyaltyRedemptionConfiguration : IEntityTypeConfiguration<LoyaltyRedemption>
{
    public void Configure(EntityTypeBuilder<LoyaltyRedemption> builder)
    {
        builder.ToTable("loyalty_redemptions");

        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(lr => lr.LoyaltyAccountId)
            .HasColumnName("loyalty_account_id")
            .IsRequired();

        builder.Property(lr => lr.PointsUsed)
            .HasColumnName("points_used")
            .IsRequired();

        builder.Property(lr => lr.RewardType)
            .HasColumnName("reward_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(lr => lr.RewardMetadata)
            .HasColumnName("reward_metadata")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(lr => lr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(lr => lr.LoyaltyAccountId)
            .HasDatabaseName("idx_loyalty_redemptions_account_id");

        builder.HasIndex(lr => lr.CreatedAt)
            .HasDatabaseName("idx_loyalty_redemptions_created_at");

        builder.HasIndex(lr => lr.RewardType)
            .HasDatabaseName("idx_loyalty_redemptions_reward_type");

        // Foreign key relationship
        builder.HasOne(lr => lr.LoyaltyAccount)
            .WithMany(la => la.LoyaltyRedemptions)
            .HasForeignKey(lr => lr.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
