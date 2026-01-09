using backend.Common.Enums;
using backend.Extensions;
using backend.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Backend.Tests.Extensions;

public class HttpContextExtensionsTests
{
    private readonly DefaultHttpContext _context;

    public HttpContextExtensionsTests()
    {
        _context = new DefaultHttpContext();
    }

    #region SetAuthenticatedUser and GetAuthenticatedUser Tests

    [Fact]
    public void SetAuthenticatedUser_StoresUserInItems()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        _context.SetAuthenticatedUser(user);

        // Assert
        _context.Items.Should().ContainKey("AuthenticatedUser");
        _context.Items["AuthenticatedUser"].Should().Be(user);
    }

    [Fact]
    public void GetAuthenticatedUser_ReturnsStoredUser()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.Chef,
            FirstName = "Test",
            LastName = "Chef"
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.GetAuthenticatedUser();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(user);
        result!.Email.Should().Be("test@example.com");
        result.Role.Should().Be(UserRole.Chef);
    }

    [Fact]
    public void GetAuthenticatedUser_WhenNotSet_ReturnsNull()
    {
        // Act
        var result = _context.GetAuthenticatedUser();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.IsAuthenticated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenUserNotSet_ReturnsFalse()
    {
        // Act
        var result = _context.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsInRole Tests

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Chef)]
    [InlineData(UserRole.Admin)]
    public void IsInRole_WhenUserHasRole_ReturnsTrue(UserRole role)
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = role
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.IsInRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_WhenUserHasDifferentRole_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var resultChef = _context.IsInRole(UserRole.Chef);
        var resultAdmin = _context.IsInRole(UserRole.Admin);

        // Assert
        resultChef.Should().BeFalse();
        resultAdmin.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_WhenUserNotSet_ReturnsFalse()
    {
        // Act
        var result = _context.IsInRole(UserRole.User);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetUserId Tests

    [Fact]
    public void GetUserId_WhenUserExists_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new AuthenticatedUser
        {
            Id = userId,
            Email = "test@example.com",
            Role = UserRole.User
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WhenUserNotSet_ReturnsNull()
    {
        // Act
        var result = _context.GetUserId();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserEmail Tests

    [Fact]
    public void GetUserEmail_WhenUserExists_ReturnsEmail()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.GetUserEmail();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void GetUserEmail_WhenUserNotSet_ReturnsNull()
    {
        // Act
        var result = _context.GetUserEmail();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserRole Tests

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Chef)]
    [InlineData(UserRole.Admin)]
    public void GetUserRole_WhenUserExists_ReturnsRole(UserRole role)
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = role
        };
        _context.SetAuthenticatedUser(user);

        // Act
        var result = _context.GetUserRole();

        // Assert
        result.Should().Be(role);
    }

    [Fact]
    public void GetUserRole_WhenUserNotSet_ReturnsNull()
    {
        // Act
        var result = _context.GetUserRole();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SetAuthenticatedUser_OverwritesPreviousUser()
    {
        // Arrange
        var user1 = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            Role = UserRole.User
        };
        var user2 = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            Role = UserRole.Admin
        };

        // Act
        _context.SetAuthenticatedUser(user1);
        _context.SetAuthenticatedUser(user2);
        var result = _context.GetAuthenticatedUser();

        // Assert
        result.Should().BeSameAs(user2);
        result!.Email.Should().Be("user2@example.com");
    }

    [Fact]
    public void AllGetters_WithNullFirstAndLastName_WorkCorrectly()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.User,
            FirstName = null,
            LastName = null
        };
        _context.SetAuthenticatedUser(user);

        // Act & Assert
        _context.GetAuthenticatedUser().Should().NotBeNull();
        _context.GetAuthenticatedUser()!.FirstName.Should().BeNull();
        _context.GetAuthenticatedUser()!.LastName.Should().BeNull();
    }

    #endregion
}
