using AutoMapper;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.DTOs.User.Request;
using backend.DTOs.User.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockMapper.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        var userResponse = new UserResponse
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockMapper.Setup(x => x.Map<UserResponse>(user))
            .Returns(userResponse);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data?.Email.Should().Be("test@example.com");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(x => x.Map<UserResponse>(user), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with ID {userId} not found");
    }

    [Fact]
    public async Task GetByIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving user");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe"
        };

        var userResponse = new UserResponse
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockMapper.Setup(x => x.Map<UserResponse>(user))
            .Returns(userResponse);

        // Act
        var result = await _userService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var email = "notfound@example.com";
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with email {email} not found");
    }

    [Fact]
    public async Task GetUserEntityByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserEntityByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserEntityByEmailAsync_ExceptionThrown_ReturnsNull()
    {
        // Arrange
        var email = "test@example.com";
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.GetUserEntityByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com" }
        };

        var userResponses = new List<UserResponse>
        {
            new UserResponse { Email = "user1@example.com" },
            new UserResponse { Email = "user2@example.com" }
        };

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserResponse>>(users))
            .Returns(userResponses);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "active@example.com", IsActive = true }
        };

        var userResponses = new List<UserResponse>
        {
            new UserResponse { Email = "active@example.com" }
        };

        _mockUserRepository.Setup(x => x.GetActiveUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserResponse>>(users))
            .Returns(userResponses);

        // Act
        var result = await _userService.GetActiveUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAdminUsersAsync_ReturnsAdminUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "admin@example.com", IsAdmin = true }
        };

        var userResponses = new List<UserResponse>
        {
            new UserResponse { Email = "admin@example.com" }
        };

        _mockUserRepository.Setup(x => x.GetAdminUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserResponse>>(users))
            .Returns(userResponses);

        // Act
        var result = await _userService.GetAdminUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_EmailDoesNotExist_CreatesUser()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = createRequest.Email,
            FirstName = createRequest.FirstName,
            LastName = createRequest.LastName
        };

        var userResponse = new UserResponse
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(createRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockMapper.Setup(x => x.Map<User>(createRequest))
            .Returns(user);

        _mockMapper.Setup(x => x.Map<UserResponse>(user))
            .Returns(userResponse);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        // Act
        var result = await _userService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(createRequest.Email);

        _mockUserRepository.Verify(x => x.EmailExistsAsync(createRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(createRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with email {createRequest.Email} already exists");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UserExists_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Old",
            LastName = "Name"
        };

        var userResponse = new UserResponse
        {
            Email = "test@example.com",
            FirstName = "Updated",
            LastName = "Name"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockMapper.Setup(x => x.Map(updateRequest, existingUser))
            .Callback<UpdateUserRequest, User>((req, user) =>
            {
                user.FirstName = req.FirstName;
                user.LastName = req.LastName;
            });

        _mockMapper.Setup(x => x.Map<UserResponse>(existingUser))
            .Returns(userResponse);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.UpdateAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.UpdateAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with ID {userId} not found");

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmailChanged_EmailDoesNotExist_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Email = "newemail@example.com",
            FirstName = "Updated",
            LastName = "Name"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "oldemail@example.com",
            FirstName = "Old",
            LastName = "Name"
        };

        var userResponse = new UserResponse
        {
            Email = "newemail@example.com",
            FirstName = "Updated",
            LastName = "Name"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(updateRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockMapper.Setup(x => x.Map(updateRequest, existingUser));
        _mockMapper.Setup(x => x.Map<UserResponse>(existingUser))
            .Returns(userResponse);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.UpdateAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(x => x.EmailExistsAsync(updateRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_EmailChanged_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Email = "existing@example.com",
            FirstName = "Updated",
            LastName = "Name"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "oldemail@example.com",
            FirstName = "Old",
            LastName = "Name"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(updateRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with email {updateRequest.Email} already exists");
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_UserExists_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(x => x.DeleteAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(x => x.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User with ID {userId} not found");
        _mockUserRepository.Verify(x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_UpdatesLastLogin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.UpdateLastLoginAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockUserRepository.Verify(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userService.UpdateLastLoginAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error updating last login");
    }
}

