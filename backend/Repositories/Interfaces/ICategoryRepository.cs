using backend.Models;

namespace backend.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<MenuCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<MenuCategory>> GetAllActiveAsync(CancellationToken ct = default);
    Task<MenuCategory> AddAsync(MenuCategory category, CancellationToken ct = default);
    Task UpdateAsync(MenuCategory category, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}