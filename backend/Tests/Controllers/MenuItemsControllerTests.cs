using backend.Common;
using backend.Controllers;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.DTOs.Menu.Request;
using backend.DTOs.Menu.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class MenuItemsControllerTests
{
    private readonly Mock<IMenuService> _mockMenuService;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IMenuItemRepository> _mockMenuItemRepo;
    private readonly MenuItemsController _controller;

    public MenuItemsControllerTests()
    {
        _mockMenuService = new Mock<IMenuService>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockMenuItemRepo = new Mock<IMenuItemRepository>();
        var httpContext = new DefaultHttpContext();
        _controller = new MenuItemsController(_mockMenuService.Object, _mockBlobStorage.Object, _mockMenuItemRepo.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var items = new List<MenuItemResponseDto>();
        _mockMenuService.Setup(x => x.GetAllMenuItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MenuItemResponseDto>>.Success(items));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockMenuService.Setup(x => x.GetAllMenuItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MenuItemResponseDto>>.Failure("Error"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItemResponseDto
        {
            Id = itemId,
            CategoryId = Guid.NewGuid(),
            Name = "Item",
            Description = "Desc",
            Price = 10m,
            Currency = "USD",
            Available = true,
            Visibility = 0,
            Components = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockMenuService.Setup(x => x.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MenuItemResponseDto>.Success(item));

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuService.Setup(x => x.GetMenuItemByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MenuItemResponseDto>.Failure("Not found"));

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByCategory_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var items = new List<MenuItemResponseDto>();
        _mockMenuService.Setup(x => x.GetMenuItemsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MenuItemResponseDto>>.Success(items));

        // Act
        var result = await _controller.GetByCategory(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenItemCreated()
    {
        // Arrange
        var dto = new CreateMenuItemDto(Guid.NewGuid(), "Item", "Desc", 10m, "USD", true, null);
        var item = new MenuItemResponseDto
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            Available = dto.Available,
            Visibility = 0,
            Components = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockMenuService.Setup(x => x.CreateMenuItemAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MenuItemResponseDto>.Success(item));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var dto = new CreateMenuItemDto(Guid.NewGuid(), "Item", null, 10m, "USD", true, null);
        _mockMenuService.Setup(x => x.CreateMenuItemAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MenuItemResponseDto>.Failure("Error"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenItemUpdated()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var dto = new CreateMenuItemDto(Guid.NewGuid(), "Updated", null, 15m, "USD", true, null);
        _mockMenuService.Setup(x => x.UpdateMenuItemAsync(itemId, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Update(itemId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenItemDeleted()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuService.Setup(x => x.DeleteMenuItemAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WhenNoImageProvided()
    {
        // Act
        var result = await _controller.UploadImage(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImage_ReturnsNotFound_WhenMenuItemNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1024);
        
        _mockMenuItemRepo.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        // Act
        var result = await _controller.UploadImage(itemId, file.Object, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadImage_ReturnsOk_WhenImageUploaded()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var menuItem = new MenuItem { Id = itemId, ImageUrl = null };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1024);
        file.Setup(f => f.FileName).Returns("image.jpg");
        file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _mockMenuItemRepo.Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItem);
        _mockBlobStorage.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), "image.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("https://example.com/image.jpg"));
        _mockMenuItemRepo.Setup(x => x.UpdateAsync(It.IsAny<MenuItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UploadImage(itemId, file.Object, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}

