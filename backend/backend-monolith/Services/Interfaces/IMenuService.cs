using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend_monolith.Common;
using backend_monolith.DTOs;

namespace backend_monolith.Services;

public interface IMenuService
{
    Task<Result<MenuItemResponseDto>> CreateMenuItemAsync(CreateMenuItemDto dto, CancellationToken ct = default);
    Task<Result<MenuItemResponseDto>> GetMenuItemByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemResponseDto>>> GetAllMenuItemsAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<MenuItemResponseDto>>> GetMenuItemsByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<Result> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto, CancellationToken ct = default);
    Task<Result> DeleteMenuItemAsync(Guid id, CancellationToken ct = default);
}