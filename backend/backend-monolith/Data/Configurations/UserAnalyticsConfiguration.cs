using backend_monolith.Modelss;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_monolith.Data.Configurations;

/// <summary>
/// Entity configuration for UserAnalytics
/// </summary>
public class UserAnalyticsConfiguration : IEntityTypeConfiguration<UserAnalytics>
{
    public void Configure(EntityTypeBuilder<UserAnalytics> builder)
    {
        builder.ToTable("user_analytics");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet");

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent");

        builder.Property(a => a.ReferrerUrl)
            .HasColumnName("referrer_url");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_user_analytics_user_id");

        builder.HasIndex(a => a.SessionId)
            .HasDatabaseName("idx_user_analytics_session_id");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("idx_user_analytics_created_at");

        // Foreign key relationship
        builder.HasOne(a => a.User)
            .WithMany(u => u.UserAnalytics)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

