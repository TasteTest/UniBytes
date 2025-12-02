using backend.Common;
using backend.Controllers;
using backend.Services.Interfaces;
using backend.DTOs.UserAnalytics.Request;
using backend.DTOs.UserAnalytics.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class UserAnalyticsControllerTests
{
    private readonly Mock<IUserAnalyticsService> _mockUserAnalyticsService;
    private readonly UserAnalyticsController _controller;

    public UserAnalyticsControllerTests()
    {
        _mockUserAnalyticsService = new Mock<IUserAnalyticsService>();
        var mockLogger = new Mock<ILogger<UserAnalyticsController>>();
        _controller = new UserAnalyticsController(_mockUserAnalyticsService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenAnalyticsExists()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        var analytics = new UserAnalyticsResponse { EventType = "page_view" };

        _mockUserAnalyticsService.Setup(x => x.GetByIdAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAnalyticsResponse>.Success(analytics));

        // Act
        var result = await _controller.GetById(analyticsId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByUserId_ReturnsOk_WhenAnalyticsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var analytics = new List<UserAnalyticsResponse> { new UserAnalyticsResponse { EventType = "click" } };

        _mockUserAnalyticsService.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserAnalyticsResponse>>.Success(analytics));

        // Act
        var result = await _controller.GetByUserId(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetBySessionId_ReturnsOk_WhenAnalyticsExist()
    {
        // Arrange
        var sessionId = "session123";
        var analytics = new List<UserAnalyticsResponse> { new UserAnalyticsResponse { SessionId = sessionId } };

        _mockUserAnalyticsService.Setup(x => x.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserAnalyticsResponse>>.Success(analytics));

        // Act
        var result = await _controller.GetBySessionId(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByEventType_ReturnsOk_WhenAnalyticsExist()
    {
        // Arrange
        var eventType = "click";
        var analytics = new List<UserAnalyticsResponse> { new UserAnalyticsResponse { EventType = eventType } };

        _mockUserAnalyticsService.Setup(x => x.GetByEventTypeAsync(eventType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserAnalyticsResponse>>.Success(analytics));

        // Act
        var result = await _controller.GetByEventType(eventType, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByDateRange_ReturnsOk_WhenAnalyticsExist()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var analytics = new List<UserAnalyticsResponse>();

        _mockUserAnalyticsService.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<UserAnalyticsResponse>>.Success(analytics));

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenAnalyticsCreated()
    {
        // Arrange
        var request = new CreateUserAnalyticsRequest
        {
            UserId = Guid.NewGuid(),
            SessionId = "session123",
            EventType = "page_view"
        };
        var response = new UserAnalyticsResponse { EventType = request.EventType };

        _mockUserAnalyticsService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAnalyticsResponse>.Success(response));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CreateUserAnalyticsRequest();
        _controller.ModelState.AddModelError("EventType", "Required");

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenAnalyticsDeleted()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        _mockUserAnalyticsService.Setup(x => x.DeleteAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(analyticsId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}

