using AutoMapper;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.DTOs.OAuthProvider.Request;
using backend.DTOs.OAuthProvider.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class OAuthProviderServiceTests
{
    private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OAuthProviderService _oauthProviderService;

    public OAuthProviderServiceTests()
    {
        _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
        _mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<OAuthProviderService>>();

        _oauthProviderService = new OAuthProviderService(
            _mockOAuthProviderRepository.Object,
            _mockMapper.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ProviderExists_ReturnsSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new OAuthProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com"
        };

        var providerResponse = new OAuthProviderResponse
        {
            Provider = OAuthProviderType.Google,
            ProviderEmail = "test@example.com"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockMapper.Setup(x => x.Map<OAuthProviderResponse>(provider))
            .Returns(providerResponse);

        // Act
        var result = await _oauthProviderService.GetByIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Provider.Should().Be(OAuthProviderType.Google);
    }

    [Fact]
    public async Task GetByIdAsync_ProviderNotFound_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        // Act
        var result = await _oauthProviderService.GetByIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"OAuth provider with ID {providerId} not found");
    }

    [Fact]
    public async Task GetByUserIdAsync_UserHasProviders_ReturnsProviders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providers = new List<OAuthProvider>
        {
            new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Google,
                ProviderId = "google123"
            },
            new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Facebook,
                ProviderId = "facebook456"
            }
        };

        var providerResponses = new List<OAuthProviderResponse>
        {
            new OAuthProviderResponse { Provider = OAuthProviderType.Google },
            new OAuthProviderResponse { Provider = OAuthProviderType.Facebook }
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        _mockMapper.Setup(x => x.Map<IEnumerable<OAuthProviderResponse>>(providers))
            .Returns(providerResponses);

        // Act
        var result = await _oauthProviderService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ProviderExists_ReturnsSuccess()
    {
        // Arrange
        var providerType = OAuthProviderType.Google;
        var providerId = "google123";
        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Provider = providerType,
            ProviderId = providerId
        };

        var providerResponse = new OAuthProviderResponse
        {
            Provider = OAuthProviderType.Google,
            ProviderEmail = "test@example.com"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            providerType, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockMapper.Setup(x => x.Map<OAuthProviderResponse>(provider))
            .Returns(providerResponse);

        // Act
        var result = await _oauthProviderService.GetByProviderAndProviderIdAsync(providerType, providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Provider.Should().Be(OAuthProviderType.Google);
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ProviderNotFound_ReturnsFailure()
    {
        // Arrange
        var providerType = OAuthProviderType.Google;
        var providerId = "notfound";

        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(
            providerType, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        // Act
        var result = await _oauthProviderService.GetByProviderAndProviderIdAsync(providerType, providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"OAuth provider {providerType} with ID {providerId} not found");
    }

    [Fact]
    public async Task CreateAsync_ProviderDoesNotExist_CreatesProvider()
    {
        // Arrange
        var createRequest = new CreateOAuthProviderRequest
        {
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com",
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = createRequest.UserId,
            Provider = createRequest.Provider,
            ProviderId = createRequest.ProviderId,
            ProviderEmail = createRequest.ProviderEmail
        };

        var providerResponse = new OAuthProviderResponse
        {
            Provider = OAuthProviderType.Google,
            ProviderEmail = createRequest.ProviderEmail
        };

        _mockOAuthProviderRepository.Setup(x => x.ExistsAsync(
            createRequest.Provider, createRequest.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockMapper.Setup(x => x.Map<OAuthProvider>(createRequest))
            .Returns(provider);

        _mockMapper.Setup(x => x.Map<OAuthProviderResponse>(provider))
            .Returns(providerResponse);

        _mockOAuthProviderRepository.Setup(x => x.AddAsync(It.IsAny<OAuthProvider>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider p, CancellationToken ct) => p);

        // Act
        var result = await _oauthProviderService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockOAuthProviderRepository.Verify(x => x.AddAsync(It.IsAny<OAuthProvider>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ProviderAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var createRequest = new CreateOAuthProviderRequest
        {
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            ProviderEmail = "test@example.com"
        };

        _mockOAuthProviderRepository.Setup(x => x.ExistsAsync(
            createRequest.Provider, createRequest.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _oauthProviderService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");

        _mockOAuthProviderRepository.Verify(x => x.AddAsync(It.IsAny<OAuthProvider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ProviderExists_UpdatesProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updateRequest = new UpdateOAuthProviderRequest
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token"
        };

        var provider = new OAuthProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123",
            AccessToken = "old_token"
        };

        var providerResponse = new OAuthProviderResponse
        {
            Provider = OAuthProviderType.Google
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockMapper.Setup(x => x.Map(updateRequest, provider));

        _mockMapper.Setup(x => x.Map<OAuthProviderResponse>(provider))
            .Returns(providerResponse);

        _mockOAuthProviderRepository.Setup(x => x.UpdateAsync(It.IsAny<OAuthProvider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _oauthProviderService.UpdateAsync(providerId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockOAuthProviderRepository.Verify(x => x.UpdateAsync(It.IsAny<OAuthProvider>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProviderNotFound_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updateRequest = new UpdateOAuthProviderRequest
        {
            AccessToken = "new_token"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        // Act
        var result = await _oauthProviderService.UpdateAsync(providerId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"OAuth provider with ID {providerId} not found");
    }

    [Fact]
    public async Task DeleteAsync_ProviderExists_DeletesProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new OAuthProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };

        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockOAuthProviderRepository.Setup(x => x.DeleteAsync(provider, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _oauthProviderService.DeleteAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockOAuthProviderRepository.Verify(x => x.DeleteAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProviderNotFound_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        // Act
        var result = await _oauthProviderService.DeleteAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"OAuth provider with ID {providerId} not found");
    }

    [Fact]
    public async Task GetByIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.GetByIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving OAuth provider");
    }

    [Fact]
    public async Task CreateAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var createRequest = new CreateOAuthProviderRequest
        {
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };

        _mockOAuthProviderRepository.Setup(x => x.ExistsAsync(
            It.IsAny<OAuthProviderType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating OAuth provider");
    }

    [Fact]
    public async Task GetByUserIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockOAuthProviderRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving OAuth providers");
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var provider = OAuthProviderType.Google;
        var providerId = "google123";
        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.GetByProviderAndProviderIdAsync(provider, providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving OAuth provider");
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        var provider = OAuthProviderType.Google;
        var providerId = "nonexistent";
        _mockOAuthProviderRepository.Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OAuthProvider?)null);

        // Act
        var result = await _oauthProviderService.GetByProviderAndProviderIdAsync(provider, providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"OAuth provider {provider} with ID {providerId} not found");
    }

    [Fact]
    public async Task UpdateAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updateRequest = new UpdateOAuthProviderRequest();

        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.UpdateAsync(providerId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error updating OAuth provider");
    }

    [Fact]
    public async Task DeleteAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockOAuthProviderRepository.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _oauthProviderService.DeleteAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error deleting OAuth provider");
    }
}

