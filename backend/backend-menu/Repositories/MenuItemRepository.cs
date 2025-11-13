using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend_menu.Data;
using backend_menu.Model;
using Microsoft.EntityFrameworkCore;

namespace backend_menu.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly MenuDbContext _context;

    public MenuItemRepository(MenuDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.Available)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId && m.Available)
            .ToListAsync(ct);
    }

    public async Task<MenuItem> AddAsync(MenuItem item, CancellationToken ct = default)
    {
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateAsync(MenuItem item, CancellationToken ct = default)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct);
        if (item != null)
        {
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }
}