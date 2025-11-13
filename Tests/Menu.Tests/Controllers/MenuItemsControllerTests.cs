using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using backend_menu.Common;
using backend_menu.Controllers;
using backend_menu.DTOs;
using backend_menu.Model;
using backend_menu.Repositories;
using backend_menu.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Menu.Tests.Controllers;

public class MenuItemsControllerTests
{
    private readonly IMenuService _mockMenuService;
    private readonly IBlobStorageService _mockBlobStorage;
    private readonly IMenuItemRepository _mockMenuItemRepo;
    private readonly MenuItemsController _controller;

    public MenuItemsControllerTests()
    {
        _mockMenuService = Substitute.For<IMenuService>();
        _mockBlobStorage = Substitute.For<IBlobStorageService>();
        _mockMenuItemRepo = Substitute.For<IMenuItemRepository>();
        _controller = new MenuItemsController(_mockMenuService, _mockBlobStorage, _mockMenuItemRepo);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithMenuItems()
    {
        // Arrange
        var items = new List<MenuItemResponseDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Pizza", "Delicious", 25m, "RON", true, 1, null, null, DateTime.UtcNow, DateTime.UtcNow)
        };

        _mockMenuService.GetAllMenuItemsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MenuItemResponseDto>>.Success(items));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOk()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MenuItemResponseDto(itemId, Guid.NewGuid(), "Pizza", "Delicious", 25m, "RON", true, 1, null, null, DateTime.UtcNow, DateTime.UtcNow);

        _mockMenuService.GetMenuItemByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Result<MenuItemResponseDto>.Success(item));

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuService.GetMenuItemByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Result<MenuItemResponseDto>.Failure("Menu item not found"));

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateMenuItemDto(Guid.NewGuid(), "Pizza", "Delicious", 25m, "RON", true, null);
        var createdItem = new MenuItemResponseDto(Guid.NewGuid(), dto.CategoryId, dto.Name, dto.Description, dto.Price, dto.Currency, dto.Available, 1, null, null, DateTime.UtcNow, DateTime.UtcNow);

        _mockMenuService.CreateMenuItemAsync(dto, Arg.Any<CancellationToken>())
            .Returns(Result<MenuItemResponseDto>.Success(createdItem));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(createdItem);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateDto = new CreateMenuItemDto(
            Guid.NewGuid(),
            "Updated Burger",
            "New recipe",
            10.99m,
            "RON",
            true,
            null
        );
        var updatedItem = new MenuItemResponseDto(
            itemId,
            updateDto.CategoryId,
            updateDto.Name,
            updateDto.Description,
            updateDto.Price,
            updateDto.Currency,
            updateDto.Available,
            1,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        _mockMenuService.UpdateMenuItemAsync(itemId, updateDto, Arg.Any<CancellationToken>())
            .Returns(Result<MenuItemResponseDto>.Success(updatedItem));

        // Act
        var result = await _controller.Update(itemId, updateDto, CancellationToken.None);

        // Assert
        // Controller returns NoContent, not Ok with updated data
        result.Should().BeOfType<NoContentResult>();
        await _mockMenuService.Received(1).UpdateMenuItemAsync(itemId, updateDto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockMenuService.DeleteMenuItemAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _controller.Delete(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _mockMenuService.Received(1).DeleteMenuItemAsync(itemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByCategory_WithValidCategoryId_ReturnsOkWithItems()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var menuItems = new List<MenuItemResponseDto>
        {
            new(Guid.NewGuid(), categoryId, "Item 1", "Description 1", 5.99m, "RON", true, 1, null, null, DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), categoryId, "Item 2", "Description 2", 7.99m, "RON", true, 1, null, null, DateTime.UtcNow, DateTime.UtcNow)
        };
        _mockMenuService.GetMenuItemsByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MenuItemResponseDto>>.Success(menuItems));

        // Act
        var result = await _controller.GetByCategory(categoryId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItems = okResult.Value.Should().BeAssignableTo<IEnumerable<MenuItemResponseDto>>().Subject;
        returnedItems.Should().HaveCount(2);
        returnedItems.Should().AllSatisfy(item => item.CategoryId.Should().Be(categoryId));
    }

    [Fact]
    public async Task UploadImage_WithValidImage_ReturnsOk()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var menuItem = new MenuItem { Id = itemId, Name = "Pizza" };
        var imageUrl = "https://blob.storage/image.jpg";

        var mockFile = Substitute.For<IFormFile>();
        mockFile.Length.Returns(1024);
        mockFile.FileName.Returns("test.jpg");
        mockFile.OpenReadStream().Returns(new MemoryStream());

        _mockMenuItemRepo.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(menuItem);

        _mockBlobStorage.UploadImageAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(imageUrl));

        // Act
        var result = await _controller.UploadImage(itemId, mockFile, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { imageUrl });
    }
}