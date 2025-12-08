using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class MenuItemRepository(ApplicationDbContext context) : IMenuItemRepository
{
    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.Available)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId && m.Available)
            .ToListAsync(ct);
    }

    public async Task<MenuItem> AddAsync(MenuItem item, CancellationToken ct = default)
    {
        context.MenuItems.Add(item);
        await context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateAsync(MenuItem item, CancellationToken ct = default)
    {
        item.UpdatedAt = DateTime.UtcNow;
        context.MenuItems.Update(item);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct);
        if (item != null)
        {
            context.MenuItems.Remove(item);
            await context.SaveChangesAsync(ct);
        }
    }
}