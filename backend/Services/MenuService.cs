using backend.Common;
using backend.Models;
using backend.Repositories;
using backend.Repositories.Interfaces;
using backend.DTOs.Menu.Request;
using backend.DTOs.Menu.Response;

namespace backend.Services;

public class MenuService(
    IMenuItemRepository menuItemRepo,
    ICategoryRepository categoryRepo,
    ILogger<MenuService> logger)
    : IMenuService
{
    public async Task<Result<MenuItemResponseDto>> CreateMenuItemAsync(CreateMenuItemDto dto, CancellationToken ct = default)
    {
        try
        {
            var category = await categoryRepo.GetByIdAsync(dto.CategoryId, ct);
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

            var created = await menuItemRepo.AddAsync(menuItem, ct);
            
            var response = MapToDto(created);
            return Result<MenuItemResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating menu item");
            return Result<MenuItemResponseDto>.Failure("Failed to create menu item");
        }
    }

    public async Task<Result<MenuItemResponseDto>> GetMenuItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await menuItemRepo.GetByIdAsync(id, ct);
        if (item == null)
            return Result<MenuItemResponseDto>.Failure("Menu item not found");

        return Result<MenuItemResponseDto>.Success(MapToDto(item));
    }

    public async Task<Result<IEnumerable<MenuItemResponseDto>>> GetAllMenuItemsAsync(CancellationToken ct = default)
    {
        var items = await menuItemRepo.GetAllAsync(ct);
        var dtos = items.Select(MapToDto);
        return Result<IEnumerable<MenuItemResponseDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<MenuItemResponseDto>>> GetMenuItemsByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        var items = await menuItemRepo.GetByCategoryIdAsync(categoryId, ct);
        var dtos = items.Select(MapToDto);
        return Result<IEnumerable<MenuItemResponseDto>>.Success(dtos);
    }

    public async Task<Result> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto, CancellationToken ct = default)
    {
        var item = await menuItemRepo.GetByIdAsync(id, ct);
        if (item == null)
            return Result.Failure("Menu item not found");

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.Currency = dto.Currency;
        item.Available = dto.Available;
        item.Components = dto.Components;
        item.CategoryId = dto.CategoryId;

        await menuItemRepo.UpdateAsync(item, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteMenuItemAsync(Guid id, CancellationToken ct = default)
    {
        await menuItemRepo.DeleteAsync(id, ct);
        return Result.Success();
    }

    private static MenuItemResponseDto MapToDto(MenuItem item) => new()
    {
        Id = item.Id,
        CategoryId = item.CategoryId,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        Currency = item.Currency,
        Available = item.Available,
        Visibility = item.Visibility,
        Components = item.Components,
        ImageUrl = item.ImageUrl,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}