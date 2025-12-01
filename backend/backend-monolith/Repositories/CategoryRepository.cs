using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend_monolith.Data;
using backend_monolith.Modelss;
using Microsoft.EntityFrameworkCore;

namespace backend_monolith.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MenuCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MenuCategories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<MenuCategory>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<MenuCategory> AddAsync(MenuCategory category, CancellationToken ct = default)
    {
        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(MenuCategory category, CancellationToken ct = default)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.MenuCategories.Update(category);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct);
        if (category != null)
        {
            _context.MenuCategories.Remove(category);
            await _context.SaveChangesAsync(ct);
        }
    }
}