using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<MenuCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.MenuCategories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<MenuCategory>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<MenuCategory> AddAsync(MenuCategory category, CancellationToken ct = default)
    {
        context.MenuCategories.Add(category);
        await context.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(MenuCategory category, CancellationToken ct = default)
    {
        category.UpdatedAt = DateTime.UtcNow;
        context.MenuCategories.Update(category);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct);
        if (category != null)
        {
            context.MenuCategories.Remove(category);
            await context.SaveChangesAsync(ct);
        }
    }
}