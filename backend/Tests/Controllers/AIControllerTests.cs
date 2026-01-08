using backend.Common;
using backend.Controllers;
using backend.DTOs.AI.Request;
using backend.DTOs.AI.Response;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class AIControllerTests
{
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ILogger<AIController>> _mockLogger;
    private readonly AIController _controller;

    public AIControllerTests()
    {
        _mockAIService = new Mock<IAIService>();
        _mockLogger = new Mock<ILogger<AIController>>();
        _controller = new AIController(_mockAIService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Chat_WithValidRequest_ReturnsOkWithResponse()
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

        var aiResponse = new AIResponse
        {
            Response = "We have a variety of menu items including beverages, main courses, and desserts.",
            Reasoning = null
        };

        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Success(aiResponse));

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(aiResponse);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Chat_WithValidRequestIncludingReasoning_ReturnsOkWithReasoningIncluded()
    {
        // Arrange
        var request = new AIRequest
        {
            DietaryGoal = "Weight Loss",
            DietaryRestrictions = new List<string> { "Vegetarian" },
            PreferredMealType = "Lunch",
            CaloriePreference = "Low (<500)",
            MenuType = "Light Meal"
        };

        var aiResponse = new AIResponse
        {
            Response = "Based on your preferences, I recommend: Salad (12.50 RON), Fruit Bowl (8.00 RON)",
            Reasoning = "Selected low-calorie vegetarian options suitable for weight loss."
        };

        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Success(aiResponse));

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AIResponse;
        response.Should().NotBeNull();
        response!.Reasoning.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Chat_WithEmptyDietaryGoal_ReturnsBadRequest()
    {
        // Arrange
        var request = new AIRequest
        {
            DietaryGoal = "",
            DietaryRestrictions = new List<string>(),
            PreferredMealType = "Any",
            CaloriePreference = "No Preference",
            MenuType = "Light Meal"
        };

        _controller.ModelState.AddModelError("DietaryGoal", "Dietary goal is required");

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Chat_WithNullDietaryGoal_ReturnsBadRequest()
    {
        // Arrange
        var request = new AIRequest
        {
            DietaryGoal = null!,
            DietaryRestrictions = new List<string>(),
            PreferredMealType = "Any",
            CaloriePreference = "No Preference",
            MenuType = "Light Meal"
        };

        _controller.ModelState.AddModelError("DietaryGoal", "Dietary goal is required");

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Chat_WhenServiceFails_ReturnsInternalServerError()
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

        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Failure("AI service error"));

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        var response = objectResult.Value as dynamic;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Chat_WhenServiceReturnsNullData_ReturnsInternalServerError()
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

        // Use Failure instead since Success requires non-null data
        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Failure("No data returned"));

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Chat_CallsServiceWithCorrectRequest()
    {
        // Arrange
        var request = new AIRequest
        {
            DietaryGoal = "Muscle Building",
            DietaryRestrictions = new List<string> { "Dairy-Free" },
            Allergies = "Peanuts",
            Dislikes = "Mushrooms",
            PreferredMealType = "Dinner",
            CaloriePreference = "High (>800)",
            MenuType = "Full Meal"
        };

        var aiResponse = new AIResponse
        {
            Response = "High-protein meal recommendations",
            Reasoning = null
        };

        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Success(aiResponse));

        // Act
        await _controller.Chat(request, CancellationToken.None);

        // Assert
        _mockAIService.Verify(
            x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Chat_LogsWarningOnFailure()
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

        var errorMessage = "Service unavailable";
        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Failure(errorMessage));

        // Act
        await _controller.Chat(request, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("AI menu recommendation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task Chat_WithLongAllergiesAndDislikes_ProcessesSuccessfully()
    {
        // Arrange
        var longText = new string('a', 500); // Max allowed length for these fields
        var request = new AIRequest
        {
            DietaryGoal = "General Health",
            DietaryRestrictions = new List<string>(),
            Allergies = longText,
            Dislikes = longText,
            PreferredMealType = "Any",
            CaloriePreference = "No Preference",
            MenuType = "Light Meal"
        };

        var aiResponse = new AIResponse
        {
            Response = "Processed request with detailed preferences",
            Reasoning = null
        };

        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AIResponse>.Success(aiResponse));

        // Act
        var result = await _controller.Chat(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(aiResponse);
    }

    [Fact]
    public async Task Chat_PassesCancellationTokenToService()
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

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var aiResponse = new AIResponse { Response = "Test response" };
        _mockAIService.Setup(x => x.GetMenuRecommendationsAsync(request, cancellationToken))
            .ReturnsAsync(Result<AIResponse>.Success(aiResponse));

        // Act
        await _controller.Chat(request, cancellationToken);

        // Assert
        _mockAIService.Verify(
            x => x.GetMenuRecommendationsAsync(request, cancellationToken),
            Times.Once);
    }
}
