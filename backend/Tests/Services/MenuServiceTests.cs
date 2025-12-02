using System.Text.Json;
using backend.Common;
using backend.DTOs;
using backend.Modelss;
using backend.Repositories;
using backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class MenuServiceTests
{
    private readonly Mock<IMenuItemRepository> _mockMenuItemRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ILogger<MenuService>> _mockLogger;
    private readonly MenuService _menuService;

    public MenuServiceTests()
    {
        _mockMenuItemRepository = new Mock<IMenuItemRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<MenuService>>();

        _menuService = new MenuService(
            _mockMenuItemRepository.Object,
            _mockCategoryRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateMenuItemAsync_ValidCategory_CreatesMenuItem()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(
            CategoryId: categoryId,
            Name: "Pizza",
            Description: "Delicious pizza",
            Price: 12.99m,
            Currency: "USD",
            Available: true,
            Components: null
        );

        var category = new MenuCategory
        {
            Id = categoryId,
            Name = "Main Dishes"
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            Available = dto.Available,
            Components = dto.Components,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockMenuItemRepository.Setup(x => x.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItem);

        // Act
        var result = await _menuService.CreateMenuItemAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Pizza");
        result.Data.Price.Should().Be(12.99m);
        result.Data.CategoryId.Should().Be(categoryId);

        _mockCategoryRepository.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMenuItemRepository.Verify(x => x.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMenuItemAsync_CategoryNotFound_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(
            CategoryId: categoryId,
            Name: "Pizza",
            Description: "Delicious pizza",
            Price: 12.99m,
            Currency: "USD",
            Available: true,
            Components: null
        );

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        // Act
        var result = await _menuService.CreateMenuItemAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Category not found");

        _mockMenuItemRepository.Verify(x => x.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMenuItemAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(
            CategoryId: categoryId,
            Name: "Pizza",
            Description: "Delicious pizza",
            Price: 12.99m,
            Currency: "USD",
            Available: true,
            Components: null
        );

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _menuService.CreateMenuItemAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to create menu item");
    }

    [Fact]
    public async Task GetMenuItemByIdAsync_ItemExists_ReturnsMenuItem()
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
            Price = 9.99m,
            Currency = "USD",
            Available = true,
            Visibility = 1,
            ImageUrl = "http://example.com/burger.jpg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMenuItemRepository.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItem);

        // Act
        var result = await _menuService.GetMenuItemByIdAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(itemId);
        result.Data.Name.Should().Be("Burger");
        result.Data.Price.Should().Be(9.99m);
    }

    [Fact]
    public async Task GetMenuItemByIdAsync_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuItemRepository.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        // Act
        var result = await _menuService.GetMenuItemByIdAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Menu item not found");
    }

    [Fact]
    public async Task GetAllMenuItemsAsync_ReturnsAllMenuItems()
    {
        // Arrange
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Name = "Pizza",
                Price = 12.99m,
                Currency = "USD",
                Available = true,
                Visibility = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Name = "Burger",
                Price = 9.99m,
                Currency = "USD",
                Available = true,
                Visibility = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItems);

        // Act
        var result = await _menuService.GetAllMenuItemsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMenuItemsByCategoryAsync_ReturnsCategoryItems()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Name = "Pizza Margherita",
                Price = 11.99m,
                Currency = "USD",
                Available = true,
                Visibility = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Name = "Pizza Pepperoni",
                Price = 12.99m,
                Currency = "USD",
                Available = true,
                Visibility = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockMenuItemRepository.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItems);

        // Act
        var result = await _menuService.GetMenuItemsByCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
        result.Data!.All(x => x.CategoryId == categoryId).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateMenuItemAsync_ItemExists_UpdatesMenuItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(
            CategoryId: categoryId,
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 14.99m,
            Currency: "USD",
            Available: false,
            Components: null
        );

        var existingItem = new MenuItem
        {
            Id = itemId,
            CategoryId = categoryId,
            Name = "Old Pizza",
            Description = "Old description",
            Price = 12.99m,
            Currency = "USD",
            Available = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockMenuItemRepository.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _mockMenuItemRepository.Setup(x => x.UpdateAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _menuService.UpdateMenuItemAsync(itemId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockMenuItemRepository.Verify(x => x.UpdateAsync(It.Is<MenuItem>(m => 
            m.Name == "Updated Pizza" && 
            m.Price == 14.99m && 
            m.Available == false), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMenuItemAsync_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(
            CategoryId: Guid.NewGuid(),
            Name: "Updated Pizza",
            Description: "Updated description",
            Price: 14.99m,
            Currency: "USD",
            Available: false,
            Components: null
        );

        _mockMenuItemRepository.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        // Act
        var result = await _menuService.UpdateMenuItemAsync(itemId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Menu item not found");

        _mockMenuItemRepository.Verify(x => x.UpdateAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMenuItemAsync_DeletesMenuItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _mockMenuItemRepository.Setup(x => x.DeleteAsync(itemId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _menuService.DeleteMenuItemAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockMenuItemRepository.Verify(x => x.DeleteAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMenuItemAsync_WithComponents_CreatesMenuItemWithComponents()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var componentsJson = JsonDocument.Parse("{\"ingredients\": [\"cheese\", \"tomato\"]}");
        
        var dto = new CreateMenuItemDto(
            CategoryId: categoryId,
            Name: "Custom Pizza",
            Description: "Build your own",
            Price: 15.99m,
            Currency: "USD",
            Available: true,
            Components: componentsJson
        );

        var category = new MenuCategory
        {
            Id = categoryId,
            Name = "Custom Items"
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            Available = dto.Available,
            Components = dto.Components,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockMenuItemRepository.Setup(x => x.AddAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItem);

        // Act
        var result = await _menuService.CreateMenuItemAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Components.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMenuItemsAsync_EmptyList_ReturnsEmptySuccess()
    {
        // Arrange
        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>());

        // Act
        var result = await _menuService.GetAllMenuItemsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenuItemsByCategoryAsync_EmptyCategory_ReturnsEmptySuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockMenuItemRepository.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>());

        // Act
        var result = await _menuService.GetMenuItemsByCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }
}

