using backend.Common;
using backend.Common.Enums;
using backend.Controllers;
using backend.DTOs.Auth.Request;
using backend.DTOs.Auth.Response;
using backend.DTOs.User.Response;
using backend.Models;
using backend.Middleware;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly AuthController _controller;
    private readonly DefaultHttpContext _httpContext;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        _httpContext = new DefaultHttpContext();
        _controller = new AuthController(_mockAuthService.Object, _mockUserService.Object, mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async Task GoogleAuth_ReturnsOk_WhenAuthenticationSucceeds()
    {
        // Arrange
        var request = new GoogleAuthRequest
        {
            Email = "test@example.com",
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            AccessToken = "token"
        };

        var authResponse = new AuthResponse
        {
            UserId = Guid.NewGuid().ToString(),
            User = new UserResponse { Email = "test@example.com" },
            IsNewUser = false,
            Message = "User authenticated successfully"
        };

        _mockAuthService.Setup(x => x.AuthenticateWithGoogleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Success(authResponse));

        // Act
        var result = await _controller.GoogleAuth(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(authResponse);
    }

    [Fact]
    public async Task GoogleAuth_ReturnsBadRequest_WhenAuthenticationFails()
    {
        // Arrange
        var request = new GoogleAuthRequest
        {
            Email = "test@example.com",
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };

        _mockAuthService.Setup(x => x.AuthenticateWithGoogleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponse>.Failure("Authentication failed"));

        // Act
        var result = await _controller.GoogleAuth(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as dynamic;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange - no AuthenticatedUser set in HttpContext.Items (simulates middleware rejection passed through)
        _httpContext.Items.Clear();

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenUserNotFoundInDatabase()
    {
        // Arrange
        var email = "notfound@example.com";
        _httpContext.Items["AuthenticatedUser"] = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = UserRole.User
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            AvatarUrl = "http://example.com/avatar.jpg",
            Role = UserRole.User
        };

        // Set authenticated user in HttpContext (simulates middleware behavior)
        _httpContext.Items["AuthenticatedUser"] = new AuthenticatedUser
        {
            Id = userId,
            Email = email,
            Role = UserRole.User
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }
}

