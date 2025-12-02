using backend.Controllers;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.DTOs.Menu.Request;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_mockCategoryRepo.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenCategoriesExist()
    {
        // Arrange
        var categories = new List<MenuCategory>
        {
            new MenuCategory { Id = Guid.NewGuid(), Name = "Category1", IsActive = true },
            new MenuCategory { Id = Guid.NewGuid(), Name = "Category2", IsActive = true }
        };

        _mockCategoryRepo.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory { Id = categoryId, Name = "Category", IsActive = true };

        _mockCategoryRepo.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetById(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockCategoryRepo.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        // Act
        var result = await _controller.GetById(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenCategoryCreated()
    {
        // Arrange
        var dto = new CreateCategoryDto("Category", "Description", 1, true);
        var createdCategory = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _mockCategoryRepo.Setup(x => x.AddAsync(It.IsAny<MenuCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory c, CancellationToken ct) => createdCategory);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created!.ActionName.Should().Be(nameof(CategoriesController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenCategoryUpdated()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new MenuCategory { Id = categoryId, Name = "Old" };
        var dto = new CreateCategoryDto("New", "Desc", 2, true);

        _mockCategoryRepo.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockCategoryRepo.Setup(x => x.UpdateAsync(It.IsAny<MenuCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(categoryId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        category.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new CreateCategoryDto("New", null, 1, true);

        _mockCategoryRepo.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        // Act
        var result = await _controller.Update(categoryId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenCategoryDeleted()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockCategoryRepo.Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockCategoryRepo.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

