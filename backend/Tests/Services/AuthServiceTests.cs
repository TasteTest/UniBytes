using AutoMapper;
using backend.Common.Enums;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.DTOs.Auth.Request;
using backend.DTOs.User.Response;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class AuthServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Use real in-memory database - InMemory provider doesn't support transactions
        // but ExecutionStrategy will work fine with it
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new ApplicationDbContext(options);
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
        _mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _context,
            _mockUserRepository.Object,
            _mockOAuthProviderRepository.Object,
            _mockMapper.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_ExistingOAuthProvider_ReturnsSuccessWithExistingUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            AvatarUrl = "http://example.com/avatar.jpg",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            Role = UserRole.User
        };

        var existingOAuthProvider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            AccessToken = "old_token",
            RefreshToken = "old_refresh",
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        var userResponse = new UserResponse
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Add entities to context so they can be tracked
        _context.Users.Add(existingUser);
        _context.Set<OAuthProvider>().Add(existingOAuthProvider);
        await _context.SaveChangesAsync();

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOAuthProvider);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockMapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Expected success but got: {result.Error}");
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId.ToString());
        result.Data.IsNewUser.Should().BeFalse();
        result.Data.User.Should().NotBeNull();
        result.Data.Message.Should().Be("User authenticated successfully");

        _mockOAuthProviderRepository.Verify(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_ExistingOAuthProviderButUserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            Email = "test@example.com",
            AccessToken = "access_token"
        };

        var existingOAuthProvider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOAuthProvider);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_NewUser_CreatesUserAndOAuthProvider()
    {
        // Arrange
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "newuser@example.com",
            Email = "newuser@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            AvatarUrl = "http://example.com/avatar.jpg",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var userResponse = new UserResponse
        {
            Email = "newuser@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockMapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Expected success but got: {result.Error}");
        result.Data.Should().NotBeNull();
        result.Data!.IsNewUser.Should().BeTrue();
        result.Data.User.Should().NotBeNull();
        result.Data.Message.Should().Be("User created successfully");

        // Verify user and OAuth provider were added to context
        _context.Users.Should().HaveCount(1);
        _context.Set<OAuthProvider>().Should().HaveCount(1);
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_ExistingUserByEmail_LinksOAuthProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var existingUser = new User
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

        // Add user to context
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockMapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Expected success but got: {result.Error}");
        result.Data.Should().NotBeNull();
        result.Data!.IsNewUser.Should().BeFalse();
        result.Data.User.Should().NotBeNull();
        result.Data.Message.Should().Be("User authenticated successfully");

        // Verify OAuth provider was added
        _context.Set<OAuthProvider>().Should().HaveCount(1);
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            Email = "test@example.com",
            AccessToken = "access_token"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            It.IsAny<OAuthProviderType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Authentication error");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task AuthenticateWithGoogleAsync_UpdatesUserProfileIfNotSet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GoogleAuthRequest
        {
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            AvatarUrl = "http://example.com/avatar.jpg",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = null, // Empty profile
            LastName = null,
            AvatarUrl = null,
            IsActive = true,
            Role = UserRole.User
        };

        var existingOAuthProvider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            AccessToken = "old_token"
        };

        var userResponse = new UserResponse
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Add entities to context
        _context.Users.Add(existingUser);
        _context.Set<OAuthProvider>().Add(existingOAuthProvider);
        await _context.SaveChangesAsync();

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            request.Provider, request.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOAuthProvider);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockMapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _authService.AuthenticateWithGoogleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue($"Expected success but got: {result.Error}");
        
        // Verify user profile was updated
        existingUser.FirstName.Should().Be("John");
        existingUser.LastName.Should().Be("Doe");
        existingUser.AvatarUrl.Should().Be("http://example.com/avatar.jpg");
    }
}
