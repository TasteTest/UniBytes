using backend.Common.Enums;
using backend.Middleware;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Middleware;

public class AuthenticatedUserTests
{
    [Fact]
    public void AuthenticatedUser_CanBeInstantiated_WithProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var role = UserRole.Chef;
        var firstName = "Test";
        var lastName = "User";

        // Act
        var user = new AuthenticatedUser
        {
            Id = id,
            Email = email,
            Role = role,
            FirstName = firstName,
            LastName = lastName
        };

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.Role.Should().Be(role);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
    }

    [Fact]
    public void AuthenticatedUser_Defaults_AreCorrect()
    {
        // Act
        var user = new AuthenticatedUser();

        // Assert
        user.Id.Should().Be(Guid.Empty);
        user.Email.Should().Be(string.Empty); // Initialized to string.Empty
        user.Role.Should().Be((UserRole)0); // Default enum value (User)
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
    }
}
