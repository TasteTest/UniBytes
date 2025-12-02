using backend.Common;
using backend.Controllers;
using backend.Services.Interfaces;
using backend.DTOs.User.Request;
using backend.DTOs.User.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithUsers_WhenSuccess()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new UserResponse { Email = "user1@example.com", FirstName = "User", LastName = "One" },
            new UserResponse { Email = "user2@example.com", FirstName = "User", LastName = "Two" }
        };

        _mockUserService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Success(users));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Failure("Error"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Error");
    }

    #endregion

    #region GetActive Tests

    [Fact]
    public async Task GetActive_ReturnsOkWithActiveUsers_WhenSuccess()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new UserResponse { Email = "active@example.com", FirstName = "Active", LastName = "User" }
        };

        _mockUserService.Setup(x => x.GetActiveUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Success(users));

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetActive_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetActiveUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Failure("Error"));

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetAdmins Tests

    [Fact]
    public async Task GetAdmins_ReturnsOkWithAdminUsers_WhenSuccess()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new UserResponse { Email = "admin@example.com", FirstName = "Admin", LastName = "User" }
        };

        _mockUserService.Setup(x => x.GetAdminUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Success(users));

        // Act
        var result = await _controller.GetAdmins(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAdmins_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetAdminUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserResponse>>.Failure("Error"));

        // Act
        var result = await _controller.GetAdmins(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ReturnsOkWithUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserResponse { Email = "test@example.com", FirstName = "Test", LastName = "User" };

        _mockUserService.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(user));

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure("User not found"));

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFound = result as NotFoundObjectResult;
        notFound!.Value.Should().Be("User not found");
    }

    #endregion

    #region GetByEmail Tests

    [Fact]
    public async Task GetByEmail_ReturnsOkWithUser_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new UserResponse { Email = email, FirstName = "Test", LastName = "User" };

        _mockUserService.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(user));

        // Act
        var result = await _controller.GetByEmail(email, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmail_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var email = "notfound@example.com";

        _mockUserService.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure("User not found"));

        // Act
        var result = await _controller.GetByEmail(email, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreated_WhenUserCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        var userResponse = new UserResponse
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        _mockUserService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(userResponse));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created!.ActionName.Should().Be(nameof(UsersController.GetById));
        created.Value.Should().BeEquivalentTo(userResponse);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CreateUserRequest();
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User"
        };

        _mockUserService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure("User already exists"));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("User already exists");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsOk_WhenUserUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var userResponse = new UserResponse
        {
            Email = "test@example.com",
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        _mockUserService.Setup(x => x.UpdateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(userResponse));

        // Act
        var result = await _controller.Update(userId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(userResponse);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest();
        _controller.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        var result = await _controller.Update(userId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest { FirstName = "Updated" };

        _mockUserService.Setup(x => x.UpdateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure("User not found"));

        // Act
        var result = await _controller.Update(userId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenUserDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("User not found"));

        // Act
        var result = await _controller.Delete(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region UpdateLastLogin Tests

    [Fact]
    public async Task UpdateLastLogin_ReturnsNoContent_WhenUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.UpdateLastLogin(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateLastLogin_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("User not found"));

        // Act
        var result = await _controller.UpdateLastLogin(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}

