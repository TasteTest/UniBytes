using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend_menu.Controllers;
using backend_menu.DTOs;
using backend_menu.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Menu.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly ICategoryRepository _mockRepository;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockRepository = Substitute.For<ICategoryRepository>();
        _controller = new CategoriesController(_mockRepository);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithCategories()
    {
        // Arrange
        var categories = new List<backend_menu.Model.MenuCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Breakfast", Description = "Morning meals", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Lunch", Description = "Afternoon meals", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        
        _mockRepository.GetAll()
            .Returns(categories);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategories = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryResponseDto>>().Subject;
        returnedCategories.Should().HaveCount(2);
        returnedCategories.Should().Contain(c => c.Name == "Breakfast");
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOkWithCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new backend_menu.Model.MenuCategory 
        { 
            Id = categoryId, 
            Name = "Desserts", 
            Description = "Sweet treats",
            DisplayOrder = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _controller.GetById(categoryId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategory = okResult.Value.Should().BeAssignableTo<CategoryResponseDto>().Subject;
        returnedCategory.Name.Should().Be("Desserts");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((backend_menu.Model.MenuCategory?)null);

        // Act
        var result = await _controller.GetById(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateCategoryDto("New Category", "Test description", 1, true);
        var createdCategory = new backend_menu.Model.MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            Description = "Test description",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockRepository.AddAsync(Arg.Any<backend_menu.Model.MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(createdCategory);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CategoriesController.GetById));
        var returnedCategory = createdResult.Value.Should().BeAssignableTo<CategoryResponseDto>().Subject;
        returnedCategory.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new backend_menu.Model.MenuCategory
        {
            Id = categoryId,
            Name = "Old Name",
            Description = "Old description",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            DisplayOrder = 2,
            Active = true
        };
        
        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(existingCategory);
        _mockRepository.UpdateAsync(Arg.Any<backend_menu.Model.MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(categoryId, updateDto, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategory = okResult.Value.Should().BeAssignableTo<CategoryResponseDto>().Subject;
        returnedCategory.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.DeleteAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(categoryId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _mockRepository.Received(1).DeleteAsync(categoryId, Arg.Any<CancellationToken>());
    }
}