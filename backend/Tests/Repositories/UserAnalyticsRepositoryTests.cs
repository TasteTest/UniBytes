using backend.Data;
using backend.Modelss;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class UserAnalyticsRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserAnalyticsRepository _repository;

    public UserAnalyticsRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new UserAnalyticsRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAnalyticsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var analytics = new List<UserAnalytics>
        {
            new UserAnalytics { Id = Guid.NewGuid(), UserId = userId, EventType = "page_view", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = userId, EventType = "click", CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = otherUserId, EventType = "page_view", CreatedAt = DateTime.UtcNow }
        };
        await _context.UserAnalytics.AddRangeAsync(analytics);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.All(a => a.UserId == userId).Should().BeTrue();
        result.First().CreatedAt.Should().BeAfter(result.Last().CreatedAt); // Ordered by descending
    }

    [Fact]
    public async Task GetBySessionIdAsync_ReturnsAnalyticsForSession()
    {
        // Arrange
        var sessionId = "session123";
        var analytics = new List<UserAnalytics>
        {
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), SessionId = sessionId, EventType = "page_view", CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), SessionId = sessionId, EventType = "click", CreatedAt = DateTime.UtcNow },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), SessionId = "other_session", EventType = "page_view", CreatedAt = DateTime.UtcNow }
        };
        await _context.UserAnalytics.AddRangeAsync(analytics);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySessionIdAsync(sessionId);

        // Assert
        result.Should().HaveCount(2);
        result.All(a => a.SessionId == sessionId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByEventTypeAsync_ReturnsAnalyticsByEventType()
    {
        // Arrange
        var analytics = new List<UserAnalytics>
        {
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "page_view", CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "page_view", CreatedAt = DateTime.UtcNow },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "click", CreatedAt = DateTime.UtcNow }
        };
        await _context.UserAnalytics.AddRangeAsync(analytics);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEventTypeAsync("page_view");

        // Assert
        result.Should().HaveCount(2);
        result.All(a => a.EventType == "page_view").Should().BeTrue();
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsAnalyticsInDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var analytics = new List<UserAnalytics>
        {
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "page_view", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "click", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new UserAnalytics { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), EventType = "page_view", CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };
        await _context.UserAnalytics.AddRangeAsync(analytics);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.All(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate).Should().BeTrue();
    }
}

