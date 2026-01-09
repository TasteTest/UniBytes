using backend.Common.Enums;
using backend.Extensions;
using backend.Middleware;
using backend.Services.Interfaces;
using backend.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Middleware;

public class JwtAuthenticationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<JwtAuthenticationMiddleware>> _mockLogger;
    private readonly JwtAuthenticationMiddleware _middleware;
    private readonly DefaultHttpContext _context;

    public JwtAuthenticationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<JwtAuthenticationMiddleware>>();
        _middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    #region Public Endpoint Tests

    [Theory]
    [InlineData("POST", "/api/auth/google")]
    [InlineData("GET", "/api/menuitems")]
    [InlineData("GET", "/api/categories")]
    [InlineData("POST", "/api/payments/webhook")]
    [InlineData("GET", "/health")]
    [InlineData("GET", "/swagger")]
    [InlineData("GET", "/swagger/index.html")]
    public async Task InvokeAsync_PublicEndpoint_CallsNextMiddleware(string method, string path)
    {
        // Arrange
        _context.Request.Method = method;
        _context.Request.Path = path;

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _mockNext.Verify(next => next(_context), Times.Once);
        _context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    #endregion

    #region Authorization Header Tests

    [Fact]
    public async Task InvokeAsync_MissingAuthorizationHeader_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        // No Authorization header set

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidFormat")]
    [InlineData("Basic abc123")]
    [InlineData("bearer")]
    public async Task InvokeAsync_InvalidAuthorizationFormat_Returns401(string authHeader)
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = authHeader;

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_EmptyBearerToken_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer ";

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    #endregion

    #region User Email Header Tests

    [Fact]
    public async Task InvokeAsync_MissingUserEmailHeader_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        // No X-User-Email header

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_EmptyUserEmailHeader_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        _context.Request.Headers["X-User-Email"] = "";

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    #endregion

    #region User Validation Tests

    [Fact]
    public async Task InvokeAsync_UserNotFound_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        _context.Request.Headers["X-User-Email"] = "user@test.com";

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_UserServiceThrowsException_Returns401()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        _context.Request.Headers["X-User-Email"] = "user@test.com";

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _mockNext.Verify(next => next(_context), Times.Never);
    }

    #endregion

    #region Successful Authentication Tests

    [Fact]
    public async Task InvokeAsync_ValidCredentials_SetsAuthenticatedUserAndCallsNext()
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        _context.Request.Headers["X-User-Email"] = "user@test.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            Role = UserRole.User,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        _context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        _mockNext.Verify(next => next(_context), Times.Once);
        
        var authenticatedUser = _context.GetAuthenticatedUser();
        authenticatedUser.Should().NotBeNull();
        authenticatedUser!.Id.Should().Be(user.Id);
        authenticatedUser.Email.Should().Be(user.Email);
        authenticatedUser.Role.Should().Be(user.Role);
        authenticatedUser.FirstName.Should().Be(user.FirstName);
        authenticatedUser.LastName.Should().Be(user.LastName);
    }

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Chef)]
    [InlineData(UserRole.Admin)]
    public async Task InvokeAsync_ValidCredentials_PreservesUserRole(UserRole role)
    {
        // Arrange
        _context.Request.Method = "GET";
        _context.Request.Path = "/api/orders";
        _context.Request.Headers.Authorization = "Bearer valid_token";
        _context.Request.Headers["X-User-Email"] = "user@test.com";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            Role = role,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _middleware.InvokeAsync(_context, _mockUserService.Object);

        // Assert
        var authenticatedUser = _context.GetAuthenticatedUser();
        authenticatedUser!.Role.Should().Be(role);
    }

    #endregion
}
