using backend.Common.Enums;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// OAuth provider repository implementation
/// </summary>
public class OAuthProviderRepository(ApplicationDbContext context)
    : Repository<OAuthProvider>(context), IOAuthProviderRepository
{
    public async Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(o => o.Provider == provider && o.ProviderId == providerId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(o => o.Provider == provider && o.ProviderId == providerId, cancellationToken);
    }
}

