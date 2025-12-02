using backend.Data;
using backend.Modelss;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new CategoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_CategoryExists_ReturnsCategoryWithMenuItems()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory
        {
            Id = categoryId,
            Name = "Category",
            IsActive = true
        };
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = "Item",
            Price = 10m
        };
        await _context.MenuCategories.AddAsync(category);
        await _context.MenuItems.AddAsync(menuItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Category");
        result.MenuItems.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CategoryNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var categories = new List<MenuCategory>
        {
            new MenuCategory { Id = Guid.NewGuid(), Name = "Active1", IsActive = true, DisplayOrder = 1 },
            new MenuCategory { Id = Guid.NewGuid(), Name = "Active2", IsActive = true, DisplayOrder = 2 },
            new MenuCategory { Id = Guid.NewGuid(), Name = "Inactive", IsActive = false, DisplayOrder = 3 }
        };
        await _context.MenuCategories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.IsActive).Should().BeTrue();
        result.First().DisplayOrder.Should().BeLessThan(result.Last().DisplayOrder);
    }

    [Fact]
    public async Task AddAsync_AddsCategory()
    {
        // Arrange
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            Description = "Description",
            IsActive = true,
            DisplayOrder = 1
        };

        // Act
        var result = await _repository.AddAsync(category);

        // Assert
        result.Should().BeSameAs(category);
        var saved = await _context.MenuCategories.FindAsync(category.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategory()
    {
        // Arrange
        var category = new MenuCategory { Id = Guid.NewGuid(), Name = "Old", IsActive = true };
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        category.Name = "New";
        category.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(category);

        // Assert
        var updated = await _context.MenuCategories.FindAsync(category.Id);
        updated!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_CategoryExists_DeletesCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory { Id = categoryId, Name = "ToDelete", IsActive = true };
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(categoryId);

        // Assert
        var deleted = await _context.MenuCategories.FindAsync(categoryId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CategoryNotExists_DoesNotThrow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var act = async () => await _repository.DeleteAsync(categoryId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

