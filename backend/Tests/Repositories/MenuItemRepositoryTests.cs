using backend.Data;
using backend.Modelss;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class MenuItemRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MenuItemRepository _repository;

    public MenuItemRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new MenuItemRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ItemExists_ReturnsItemWithCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory { Id = categoryId, Name = "Category", IsActive = true };
        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            CategoryId = categoryId,
            Name = "Item",
            Price = 10m,
            Available = true
        };
        await _context.MenuCategories.AddAsync(category);
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Item");
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ItemNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyAvailableItems()
    {
        // Arrange
        var category = new MenuCategory { Id = Guid.NewGuid(), Name = "Category", IsActive = true };
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), CategoryId = category.Id, Name = "Available1", Available = true },
            new MenuItem { Id = Guid.NewGuid(), CategoryId = category.Id, Name = "Available2", Available = true },
            new MenuItem { Id = Guid.NewGuid(), CategoryId = category.Id, Name = "Unavailable", Available = false }
        };
        await _context.MenuCategories.AddAsync(category);
        await _context.MenuItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(i => i.Available).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ReturnsItemsForCategory()
    {
        // Arrange
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var category1 = new MenuCategory { Id = categoryId1, Name = "Category1", IsActive = true };
        var category2 = new MenuCategory { Id = categoryId2, Name = "Category2", IsActive = true };
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), CategoryId = categoryId1, Name = "Item1", Available = true },
            new MenuItem { Id = Guid.NewGuid(), CategoryId = categoryId1, Name = "Item2", Available = true },
            new MenuItem { Id = Guid.NewGuid(), CategoryId = categoryId2, Name = "Item3", Available = true }
        };
        await _context.MenuCategories.AddRangeAsync(new[] { category1, category2 });
        await _context.MenuItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryIdAsync(categoryId1);

        // Assert
        result.Should().HaveCount(2);
        result.All(i => i.CategoryId == categoryId1 && i.Available).Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_AddsMenuItem()
    {
        // Arrange
        var category = new MenuCategory { Id = Guid.NewGuid(), Name = "Category", IsActive = true };
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "New Item",
            Price = 15m,
            Available = true
        };

        // Act
        var result = await _repository.AddAsync(item);

        // Assert
        result.Should().BeSameAs(item);
        var saved = await _context.MenuItems.FindAsync(item.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMenuItem()
    {
        // Arrange
        var category = new MenuCategory { Id = Guid.NewGuid(), Name = "Category", IsActive = true };
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Name = "Old",
            Price = 10m,
            Available = true
        };
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();

        // Act
        item.Name = "New";
        item.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(item);

        // Assert
        var updated = await _context.MenuItems.FindAsync(item.Id);
        updated!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_ItemExists_DeletesItem()
    {
        // Arrange
        var category = new MenuCategory { Id = Guid.NewGuid(), Name = "Category", IsActive = true };
        await _context.MenuCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var itemId = Guid.NewGuid();
        var item = new MenuItem
        {
            Id = itemId,
            CategoryId = category.Id,
            Name = "ToDelete",
            Price = 10m,
            Available = true
        };
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(itemId);

        // Assert
        var deleted = await _context.MenuItems.FindAsync(itemId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ItemNotExists_DoesNotThrow()
    {
        // Act
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }
}

