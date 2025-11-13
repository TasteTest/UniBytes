using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend_menu.Common;
using backend_menu.DTOs;
using backend_menu.Model;
using backend_menu.Repositories;
using backend_menu.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Menu.Tests.Services;

public class MenuServiceTests
{
    private readonly IMenuItemRepository _mockMenuItemRepo;
    private readonly ICategoryRepository _mockCategoryRepo;
    private readonly ILogger<MenuService> _mockLogger;
    private readonly MenuService _sut;

    public MenuServiceTests()
    {
        _mockMenuItemRepo = Substitute.For<IMenuItemRepository>();
        _mockCategoryRepo = Substitute.For<ICategoryRepository>();
        _mockLogger = Substitute.For<ILogger<MenuService>>();
        _sut = new MenuService(_mockMenuItemRepo, _mockCategoryRepo, _mockLogger);
    }

    [Fact]
    public async Task CreateMenuItemAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory { Id = categoryId, Name = "Main Course" };
        var dto = new CreateMenuItemDto(categoryId, "Pizza Margherita", "Classic Italian pizza", 25.99m, "RON", true, null);

        _mockCategoryRepo.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        
        _mockMenuItemRepo.AddAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<MenuItem>()));

        // Act
        var result = await _sut.CreateMenuItemAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Pizza Margherita");
        result.Value.Price.Should().Be(25.99m);
        result.Value.Currency.Should().Be("RON");
        
        await _mockMenuItemRepo.Received(1).AddAsync(
            Arg.Is<MenuItem>(m => m.Name == "Pizza Margherita"), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateMenuItemAsync_WithInvalidCategory_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(categoryId, "Pizza", "Delicious", 12.99m, "RON", true, null);

        _mockCategoryRepo.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((MenuCategory?)null);

        // Act
        var result = await _sut.CreateMenuItemAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Category not found");
        
        await _mockMenuItemRepo.DidNotReceive().AddAsync(
            Arg.Any<MenuItem>(), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetMenuItemByIdAsync_WhenExists_ReturnsMenuItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = itemId,
            CategoryId = categoryId,
            Name = "Burger",
            Description = "Tasty burger",
            Price = 18.50m,
            Currency = "RON",
            Available = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMenuItemRepo.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(menuItem);

        // Act
        var result = await _sut.GetMenuItemByIdAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Burger");
        result.Value.Price.Should().Be(18.50m);
    }

    [Fact]
    public async Task GetMenuItemByIdAsync_WhenNotExists_ReturnsFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuItemRepo.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((MenuItem?)null);

        // Act
        var result = await _sut.GetMenuItemByIdAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Menu item not found");
    }

    [Fact]
    public async Task GetAllMenuItemsAsync_ReturnsAllItems()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Pizza", Price = 25m, Currency = "RON", Available = true },
            new() { Id = Guid.NewGuid(), Name = "Pasta", Price = 20m, Currency = "RON", Available = true }
        };

        _mockMenuItemRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(items);

        // Act
        var result = await _sut.GetAllMenuItemsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(x => x.Name == "Pizza");
        result.Value.Should().Contain(x => x.Name == "Pasta");
    }

    [Fact]
    public async Task GetMenuItemsByCategoryAsync_ReturnsFilteredItems()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var items = new List<MenuItem>
        {
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Pizza", Price = 25m, Currency = "RON", Available = true },
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, Name = "Calzone", Price = 22m, Currency = "RON", Available = true }
        };

        _mockMenuItemRepo.GetByCategoryIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(items);

        // Act
        var result = await _sut.GetMenuItemsByCategoryAsync(categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(x => x.CategoryId == categoryId);
    }

    [Fact]
    public async Task UpdateMenuItemAsync_WhenExists_UpdatesSuccessfully()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingItem = new MenuItem
        {
            Id = itemId,
            CategoryId = categoryId,
            Name = "Old Name",
            Price = 10m,
            Currency = "RON",
            Available = true
        };

        var updateDto = new CreateMenuItemDto(categoryId, "New Name", "New description", 15m, "RON", false, null);

        _mockMenuItemRepo.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        // Act
        var result = await _sut.UpdateMenuItemAsync(itemId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        await _mockMenuItemRepo.Received(1).UpdateAsync(
            Arg.Is<MenuItem>(m => 
                m.Name == "New Name" && 
                m.Price == 15m && 
                m.Available == false
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateMenuItemAsync_WhenNotExists_ReturnsFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateDto = new CreateMenuItemDto(Guid.NewGuid(), "Name", "Desc", 10m, "RON", true, null);

        _mockMenuItemRepo.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((MenuItem?)null);

        // Act
        var result = await _sut.UpdateMenuItemAsync(itemId, updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Menu item not found");
    }

    [Fact]
    public async Task DeleteMenuItemAsync_CallsRepositoryDelete()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        // Act
        var result = await _sut.DeleteMenuItemAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _mockMenuItemRepo.Received(1).DeleteAsync(itemId, Arg.Any<CancellationToken>());
    }
}