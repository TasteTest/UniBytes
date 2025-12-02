using backend.Common;
using backend.Common.Enums;
using backend.Controllers;
using backend.Services.Interfaces;
using backend.DTOs.OAuthProvider.Request;
using backend.DTOs.OAuthProvider.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class OAuthProvidersControllerTests
{
    private readonly Mock<IOAuthProviderService> _mockOAuthProviderService;
    private readonly OAuthProvidersController _controller;

    public OAuthProvidersControllerTests()
    {
        _mockOAuthProviderService = new Mock<IOAuthProviderService>();
        var mockLogger = new Mock<ILogger<OAuthProvidersController>>();
        _controller = new OAuthProvidersController(_mockOAuthProviderService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProviderExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new OAuthProviderResponse { Provider = OAuthProviderType.Google };

        _mockOAuthProviderService.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OAuthProviderResponse>.Success(provider));

        // Act
        var result = await _controller.GetById(providerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProviderNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderService.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OAuthProviderResponse>.Failure("Not found"));

        // Act
        var result = await _controller.GetById(providerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByUserId_ReturnsOk_WhenProvidersExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providers = new List<OAuthProviderResponse> { new OAuthProviderResponse { Provider = OAuthProviderType.Google } };

        _mockOAuthProviderService.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OAuthProviderResponse>>.Success(providers));

        // Act
        var result = await _controller.GetByUserId(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByProviderAndProviderId_ReturnsOk_WhenProviderExists()
    {
        // Arrange
        var provider = OAuthProviderType.Google;
        var providerId = "google123";
        var response = new OAuthProviderResponse { Provider = provider };

        _mockOAuthProviderService.Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OAuthProviderResponse>.Success(response));

        // Act
        var result = await _controller.GetByProviderAndProviderId(provider, providerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenProviderCreated()
    {
        // Arrange
        var request = new CreateOAuthProviderRequest
        {
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };
        var response = new OAuthProviderResponse { Provider = OAuthProviderType.Google };

        _mockOAuthProviderService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OAuthProviderResponse>.Success(response));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CreateOAuthProviderRequest();
        _controller.ModelState.AddModelError("UserId", "Required");

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenProviderUpdated()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateOAuthProviderRequest { AccessToken = "new_token" };
        var response = new OAuthProviderResponse { Provider = OAuthProviderType.Google };

        _mockOAuthProviderService.Setup(x => x.UpdateAsync(providerId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OAuthProviderResponse>.Success(response));

        // Act
        var result = await _controller.Update(providerId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProviderDeleted()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderService.Setup(x => x.DeleteAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(providerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}

