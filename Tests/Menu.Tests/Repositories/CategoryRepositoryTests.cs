using System;
using System.Linq;
using System.Threading.Tasks;
using backend_menu.Data;
using backend_menu.Model;
using backend_menu.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Menu.Tests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsCategory()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Desserts",
            Description = "Sweet treats",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Desserts");
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var activeCategory = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inactiveCategory = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Inactive",
            DisplayOrder = 2,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.AddRange(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(c => c.IsActive);
        result.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task AddAsync_AddsCategory()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            DisplayOrder = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(category);

        // Assert
        var saved = await _context.MenuCategories.FindAsync(category.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategory()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        // Detach to simulate fresh retrieval
        _context.Entry(category).State = EntityState.Detached;

        // Act
        category.Name = "Updated";
        category.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(category);

        // Assert
        var updated = await _context.MenuCategories.FindAsync(category.Id);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuCategories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(category.Id);

        // Assert
        var deleted = await _context.MenuCategories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}