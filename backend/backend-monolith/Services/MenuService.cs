using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend_monolith.Common;
using backend_monolith.DTOs;
using backend_monolith.Modelss;
using backend_monolith.Repositories;
using Microsoft.Extensions.Logging;

namespace backend_monolith.Services;

public class MenuService : IMenuService
{
    private readonly IMenuItemRepository _menuItemRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        IMenuItemRepository menuItemRepo,
        ICategoryRepository categoryRepo,
        ILogger<MenuService> logger)
    {
        _menuItemRepo = menuItemRepo;
        _categoryRepo = categoryRepo;
        _logger = logger;
    }

    public async Task<Result<MenuItemResponseDto>> CreateMenuItemAsync(CreateMenuItemDto dto, CancellationToken ct = default)
    {
        try
        {
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId, ct);
            if (category == null)
                return Result<MenuItemResponseDto>.Failure("Category not found");

            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Currency = dto.Currency,
                Available = dto.Available,
                Components = dto.Components,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _menuItemRepo.AddAsync(menuItem, ct);
            
            var response = MapToDto(created);
            return Result<MenuItemResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return Result<MenuItemResponseDto>.Failure("Failed to create menu item");
        }
    }

    public async Task<Result<MenuItemResponseDto>> GetMenuItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _menuItemRepo.GetByIdAsync(id, ct);
        if (item == null)
            return Result<MenuItemResponseDto>.Failure("Menu item not found");

        return Result<MenuItemResponseDto>.Success(MapToDto(item));
    }

    public async Task<Result<IEnumerable<MenuItemResponseDto>>> GetAllMenuItemsAsync(CancellationToken ct = default)
    {
        var items = await _menuItemRepo.GetAllAsync(ct);
        var dtos = items.Select(MapToDto);
        return Result<IEnumerable<MenuItemResponseDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<MenuItemResponseDto>>> GetMenuItemsByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        var items = await _menuItemRepo.GetByCategoryIdAsync(categoryId, ct);
        var dtos = items.Select(MapToDto);
        return Result<IEnumerable<MenuItemResponseDto>>.Success(dtos);
    }

    public async Task<Result> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto, CancellationToken ct = default)
    {
        var item = await _menuItemRepo.GetByIdAsync(id, ct);
        if (item == null)
            return Result.Failure("Menu item not found");

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.Currency = dto.Currency;
        item.Available = dto.Available;
        item.Components = dto.Components;
        item.CategoryId = dto.CategoryId;

        await _menuItemRepo.UpdateAsync(item, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteMenuItemAsync(Guid id, CancellationToken ct = default)
    {
        await _menuItemRepo.DeleteAsync(id, ct);
        return Result.Success();
    }

    private static MenuItemResponseDto MapToDto(MenuItem item) => new(
        item.Id,
        item.CategoryId,
        item.Name,
        item.Description,
        item.Price,
        item.Currency,
        item.Available,
        item.Visibility,
        item.Components,
        item.ImageUrl,
        item.CreatedAt,
        item.UpdatedAt
    );
}