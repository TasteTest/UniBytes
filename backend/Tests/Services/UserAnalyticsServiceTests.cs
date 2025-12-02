using AutoMapper;
using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class UserAnalyticsServiceTests
{
    private readonly Mock<IUserAnalyticsRepository> _mockUserAnalyticsRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UserAnalyticsService>> _mockLogger;
    private readonly UserAnalyticsService _userAnalyticsService;

    public UserAnalyticsServiceTests()
    {
        _mockUserAnalyticsRepository = new Mock<IUserAnalyticsRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserAnalyticsService>>();

        _userAnalyticsService = new UserAnalyticsService(
            _mockUserAnalyticsRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_AnalyticsExists_ReturnsSuccess()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        var analytics = new UserAnalytics
        {
            Id = analyticsId,
            UserId = Guid.NewGuid(),
            SessionId = "session123",
            EventType = "page_view",
            EventData = "{}"
        };

        var analyticsResponse = new UserAnalyticsResponse
        {
            SessionId = "session123",
            EventType = "page_view"
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetByIdAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        _mockMapper.Setup(x => x.Map<UserAnalyticsResponse>(analytics))
            .Returns(analyticsResponse);

        // Act
        var result = await _userAnalyticsService.GetByIdAsync(analyticsId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().Be("session123");
    }

    [Fact]
    public async Task GetByIdAsync_AnalyticsNotFound_ReturnsFailure()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        _mockUserAnalyticsRepository.Setup(x => x.GetByIdAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAnalytics?)null);

        // Act
        var result = await _userAnalyticsService.GetByIdAsync(analyticsId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User analytics with ID {analyticsId} not found");
    }

    [Fact]
    public async Task GetByUserIdAsync_UserHasAnalytics_ReturnsAnalytics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var analyticsList = new List<UserAnalytics>
        {
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventType = "login",
                SessionId = "session1"
            },
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventType = "page_view",
                SessionId = "session1"
            }
        };

        var analyticsResponses = new List<UserAnalyticsResponse>
        {
            new UserAnalyticsResponse { EventType = "login" },
            new UserAnalyticsResponse { EventType = "page_view" }
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsList);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserAnalyticsResponse>>(analyticsList))
            .Returns(analyticsResponses);

        // Act
        var result = await _userAnalyticsService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBySessionIdAsync_SessionHasAnalytics_ReturnsAnalytics()
    {
        // Arrange
        var sessionId = "session123";
        var analyticsList = new List<UserAnalytics>
        {
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = sessionId,
                EventType = "click"
            }
        };

        var analyticsResponses = new List<UserAnalyticsResponse>
        {
            new UserAnalyticsResponse { SessionId = sessionId }
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsList);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserAnalyticsResponse>>(analyticsList))
            .Returns(analyticsResponses);

        // Act
        var result = await _userAnalyticsService.GetBySessionIdAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByEventTypeAsync_EventTypeExists_ReturnsAnalytics()
    {
        // Arrange
        var eventType = "purchase";
        var analyticsList = new List<UserAnalytics>
        {
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = eventType
            },
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = eventType
            }
        };

        var analyticsResponses = new List<UserAnalyticsResponse>
        {
            new UserAnalyticsResponse { EventType = eventType },
            new UserAnalyticsResponse { EventType = eventType }
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetByEventTypeAsync(eventType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsList);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserAnalyticsResponse>>(analyticsList))
            .Returns(analyticsResponses);

        // Act
        var result = await _userAnalyticsService.GetByEventTypeAsync(eventType);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsAnalyticsInRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var analyticsList = new List<UserAnalytics>
        {
            new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = "view",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        var analyticsResponses = new List<UserAnalyticsResponse>
        {
            new UserAnalyticsResponse { }
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetByDateRangeAsync(
            startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsList);

        _mockMapper.Setup(x => x.Map<IEnumerable<UserAnalyticsResponse>>(analyticsList))
            .Returns(analyticsResponses);

        // Act
        var result = await _userAnalyticsService.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAnalytics()
    {
        // Arrange
        var createRequest = new CreateUserAnalyticsRequest
        {
            UserId = Guid.NewGuid(),
            SessionId = "session123",
            EventType = "click",
            EventData = "{}",
            UserAgent = "Mozilla/5.0"
        };

        var analytics = new UserAnalytics
        {
            Id = Guid.NewGuid(),
            UserId = createRequest.UserId,
            SessionId = createRequest.SessionId,
            EventType = createRequest.EventType,
            CreatedAt = DateTime.UtcNow
        };

        var analyticsResponse = new UserAnalyticsResponse
        {
            SessionId = createRequest.SessionId,
            EventType = createRequest.EventType
        };

        _mockMapper.Setup(x => x.Map<UserAnalytics>(createRequest))
            .Returns(analytics);

        _mockMapper.Setup(x => x.Map<UserAnalyticsResponse>(analytics))
            .Returns(analyticsResponse);

        _mockUserAnalyticsRepository.Setup(x => x.AddAsync(It.IsAny<UserAnalytics>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAnalytics a, CancellationToken ct) => a);

        // Act
        var result = await _userAnalyticsService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockUserAnalyticsRepository.Verify(x => x.AddAsync(It.IsAny<UserAnalytics>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AnalyticsExists_DeletesAnalytics()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        var analytics = new UserAnalytics
        {
            Id = analyticsId,
            UserId = Guid.NewGuid(),
            EventType = "test"
        };

        _mockUserAnalyticsRepository.Setup(x => x.GetByIdAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        _mockUserAnalyticsRepository.Setup(x => x.DeleteAsync(analytics, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userAnalyticsService.DeleteAsync(analyticsId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockUserAnalyticsRepository.Verify(x => x.DeleteAsync(analytics, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AnalyticsNotFound_ReturnsFailure()
    {
        // Arrange
        var analyticsId = Guid.NewGuid();
        _mockUserAnalyticsRepository.Setup(x => x.GetByIdAsync(analyticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAnalytics?)null);

        // Act
        var result = await _userAnalyticsService.DeleteAsync(analyticsId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User analytics with ID {analyticsId} not found");
    }

    [Fact]
    public async Task GetByUserIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserAnalyticsRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userAnalyticsService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving user analytics");
    }

    [Fact]
    public async Task CreateAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var createRequest = new CreateUserAnalyticsRequest
        {
            UserId = Guid.NewGuid(),
            EventType = "test"
        };

        _mockMapper.Setup(x => x.Map<UserAnalytics>(createRequest))
            .Returns(new UserAnalytics());

        _mockUserAnalyticsRepository.Setup(x => x.AddAsync(It.IsAny<UserAnalytics>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userAnalyticsService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating user analytics");
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt()
    {
        // Arrange
        var createRequest = new CreateUserAnalyticsRequest
        {
            UserId = Guid.NewGuid(),
            EventType = "test"
        };

        UserAnalytics? capturedAnalytics = null;
        _mockUserAnalyticsRepository.Setup(x => x.AddAsync(It.IsAny<UserAnalytics>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAnalytics a, CancellationToken ct) => { capturedAnalytics = a; return a; });

        _mockMapper.Setup(x => x.Map<UserAnalytics>(createRequest))
            .Returns(new UserAnalytics());

        _mockMapper.Setup(x => x.Map<UserAnalyticsResponse>(It.IsAny<UserAnalytics>()))
            .Returns(new UserAnalyticsResponse());

        // Act
        await _userAnalyticsService.CreateAsync(createRequest);

        // Assert
        capturedAnalytics.Should().NotBeNull();
        capturedAnalytics!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task GetBySessionIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var sessionId = "session123";
        _mockUserAnalyticsRepository.Setup(x => x.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userAnalyticsService.GetBySessionIdAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving user analytics");
    }
}

