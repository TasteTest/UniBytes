using System.Net;
using System.Text.Json;
using backend.DTOs.AI.Request;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Services;

public class AIServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AIService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IMenuItemRepository> _mockMenuItemRepository;
    private readonly AIService _aiService;

    public AIServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AIService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockMenuItemRepository = new Mock<IMenuItemRepository>();

        // Setup configuration for API key
        _mockConfiguration.Setup(x => x["OpenRouter:ApiKey"])
            .Returns("test_api_key");

        // Setup menu items
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Espresso",
                Description = "Strong coffee",
                Price = 5.50m,
                Currency = "ron",
                Available = true,
                Category = new MenuCategory { Name = "Beverages" }
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Burger",
                Description = "Juicy beef burger",
                Price = 25.00m,
                Currency = "ron",
                Available = true,
                Category = new MenuCategory { Name = "Main Courses" }
            }
        };

        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItems);

        // Setup HttpClient with mocked message handler
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient("OpenRouter"))
            .Returns(httpClient);

        _aiService = new AIService(
            _mockHttpClientFactory.Object, 
            _mockConfiguration.Object, 
            _mockLogger.Object,
            _mockMenuItemRepository.Object);
    }

    [Fact]
    public async Task GetMenuRecommendationsAsync_SuccessfulApiCall_ReturnsSuccessWithResponse()
    {
        // Arrange
        var request = new AIRequest
        {
            DietaryGoal = "General Health",
            DietaryRestrictions = new List<string>(),
            PreferredMealType = "Any",
            CaloriePreference = "No Preference",
            MenuType = "Light Meal"
        };

        var apiResponse = new
        {
            id = "test-id",
            model = "openai/gpt-oss-20b:free",
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = "We have a variety of menu items including beverages, main courses, and desserts.",
                        reasoning = (string?)null
                    },
                    finish_reason = "stop"
                }
            }
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse))
            });

        // Act
        var result = await _aiService.GetMenuRecommendationsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Response.Should().Contain("menu items");
    }

    [Fact]
    public void Constructor_NoApiKeyConfigured_ThrowsException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["OpenRouter:ApiKey"]).Returns((string?)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new AIService(
                _mockHttpClientFactory.Object, 
                mockConfig.Object, 
                _mockLogger.Object,
                _mockMenuItemRepository.Object));
        
        exception.Message.Should().Contain("OpenRouter API key is not configured");
    }


    [Fact]
    public async Task GetMenuRecommendationsAsync_ResponseWithProductIds_ParsesAndReturnsProducts()
    {
        // Arrange
        var request = new AIRequest { MenuType = "Light Meal" };
        var productId = Guid.NewGuid();
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = productId,
                Name = "Test Item",
                Price = 10.0m,
                Category = new MenuCategory { Name = "Test" },
                Currency = "RON",
                Available = true
            }
        };

        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItems);

        var apiResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = $"Here is a recommendation.\nPRODUCT_IDS: {productId}"
                    }
                }
            }
        };

        SetupOpenRouterResponse(HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _aiService.GetMenuRecommendationsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Response.Should().Be("Here is a recommendation.");
        result.Data.RecommendedProducts.Should().HaveCount(1);
        result.Data.RecommendedProducts.First().Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetMenuRecommendationsAsync_ResponseWithInvalidProductIds_IgnoresInvalidIds()
    {
        // Arrange
        var request = new AIRequest { MenuType = "Light Meal" };
        var validId = Guid.NewGuid();
        var menuItems = new List<MenuItem>
        {
            new MenuItem
            {
                Id = validId,
                Name = "Valid Item",
                Price = 10.0m,
                Category = new MenuCategory { Name = "Test" },
                Currency = "RON", 
                Available = true
            }
        };

        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItems);

        var apiResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = $"Recommendation.\nPRODUCT_IDS: {validId}, invalid-guid, {Guid.Empty}"
                    }
                }
            }
        };

        SetupOpenRouterResponse(HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _aiService.GetMenuRecommendationsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.RecommendedProducts.Should().HaveCount(1);
        result.Data.RecommendedProducts.First().Id.Should().Be(validId);
    }

    [Fact]
    public async Task GetMenuRecommendationsAsync_ResponseWithIdsNotFoundInMenu_IgnoresMissingProducts()
    {
        // Arrange
        var request = new AIRequest { MenuType = "Light Meal" };
        var unknownId = Guid.NewGuid();
        
        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>()); // Empty menu

        var apiResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = $"Recommendation.\nPRODUCT_IDS: {unknownId}"
                    }
                }
            }
        };

        SetupOpenRouterResponse(HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _aiService.GetMenuRecommendationsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.RecommendedProducts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenuRecommendationsAsync_ResponseWithoutProductIds_ReturnsEmptyProductList()
    {
        // Arrange
        var request = new AIRequest { MenuType = "Light Meal" };
        
        _mockMenuItemRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>());

        var apiResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = "Just a text response without IDs."
                    }
                }
            }
        };

        SetupOpenRouterResponse(HttpStatusCode.OK, apiResponse);

        // Act
        var result = await _aiService.GetMenuRecommendationsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Response.Should().Be("Just a text response without IDs.");
        result.Data.RecommendedProducts.Should().BeEmpty();
    }

    private void SetupOpenRouterResponse(HttpStatusCode statusCode, object responseContent)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
    }
}
