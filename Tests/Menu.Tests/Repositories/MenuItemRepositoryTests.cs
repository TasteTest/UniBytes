using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using backend_menu.Data;
using backend_menu.Model;
using backend_menu.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Menu.Tests.Repositories;

public class MenuItemRepositoryTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly MenuItemRepository _repository;

    public MenuItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
        _repository = new MenuItemRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsMenuItem()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Beverages",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Coffee",
            Description = "Fresh brew",
            Price = 3.50m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(menuItem.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Coffee");
        result.Price.Should().Be(3.50m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMenuItems()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Main Dishes",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var item1 = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Burger",
            Price = 8.99m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var item2 = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Pizza",
            Price = 12.99m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        _context.MenuItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(i => i.Name == "Burger");
        result.Should().Contain(i => i.Name == "Pizza");
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsItemsInCategory()
    {
        // Arrange
        var category1 = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Drinks",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var category2 = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            DisplayOrder = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var drinkItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category1.Id,
            Name = "Soda",
            Price = 2.50m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var foodItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category2.Id,
            Name = "Sandwich",
            Price = 6.99m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.AddRange(category1, category2);
        _context.MenuItems.AddRange(drinkItem, foodItem);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).Where(m => m.CategoryId == category1.Id).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Soda");
        result.First().CategoryId.Should().Be(category1.Id);
    }

    [Fact]
    public async Task AddAsync_AddsMenuItem()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Appetizers",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Wings",
            Description = "Spicy wings",
            Price = 7.99m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AddAsync(menuItem);

        // Assert
        var saved = await _context.MenuItems.FindAsync(menuItem.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Wings");
        saved.Price.Should().Be(7.99m);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMenuItem()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Sides",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Fries",
            Price = 3.00m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        // Detach to simulate fresh retrieval
        _context.Entry(menuItem).State = EntityState.Detached;

        // Act
        menuItem.Name = "Sweet Potato Fries";
        menuItem.Price = 4.50m;
        menuItem.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(menuItem);

        // Assert
        var updated = await _context.MenuItems.FindAsync(menuItem.Id);
        updated!.Name.Should().Be("Sweet Potato Fries");
        updated.Price.Should().Be(4.50m);
    }

    [Fact]
    public async Task DeleteAsync_RemovesMenuItem()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Desserts",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Ice Cream",
            Price = 4.00m,
            Currency = "USD",
            Available = true,
            Components = JsonDocument.Parse("[]"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(menuItem.Id);

        // Assert
        var deleted = await _context.MenuItems.FindAsync(menuItem.Id);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}