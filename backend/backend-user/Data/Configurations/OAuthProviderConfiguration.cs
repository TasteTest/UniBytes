using backend_user.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_user.Data.Configurations;

/// <summary>
/// Entity configuration for OAuthProvider
/// </summary>
public class OAuthProviderConfiguration : IEntityTypeConfiguration<OAuthProvider>
{
    public void Configure(EntityTypeBuilder<OAuthProvider> builder)
    {
        builder.ToTable("oauth_providers");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.Provider)
            .HasColumnName("provider")
            .IsRequired();

        builder.Property(o => o.ProviderId)
            .HasColumnName("provider_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.ProviderEmail)
            .HasColumnName("provider_email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.AccessToken)
            .HasColumnName("access_token")
            .IsRequired();

        builder.Property(o => o.RefreshToken)
            .HasColumnName("refresh_token");

        builder.Property(o => o.TokenExpiresAt)
            .HasColumnName("token_expires_at");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Unique constraint on provider + provider_id
        builder.HasIndex(o => new { o.Provider, o.ProviderId })
            .IsUnique();

        // Index on user_id
        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("idx_oauth_providers_user_id");

        // Foreign key relationship
        builder.HasOne(o => o.User)
            .WithMany(u => u.OAuthProviders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

