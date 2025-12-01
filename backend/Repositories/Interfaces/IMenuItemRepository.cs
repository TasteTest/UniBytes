using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend.Modelss;

namespace backend.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<MenuItem> AddAsync(MenuItem item, CancellationToken ct = default);
    Task UpdateAsync(MenuItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}