using backend.Data;
using backend.Models;
using backend.Repositories;
using backend.Common.Enums;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new OrderRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOrdersWithItems()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PlacedAt = DateTime.UtcNow,
            OrderStatus = (int)OrderStatus.Pending,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m }
            }
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        var fetchedOrder = result.First();
        fetchedOrder.OrderItems.Should().NotBeNull();
        fetchedOrder.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOrdersForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = userId, PlacedAt = DateTime.UtcNow, OrderStatus = (int)OrderStatus.Pending },
            new Order { Id = Guid.NewGuid(), UserId = userId, PlacedAt = DateTime.UtcNow.AddHours(-1), OrderStatus = (int)OrderStatus.Completed },
            new Order { Id = Guid.NewGuid(), UserId = otherUserId, PlacedAt = DateTime.UtcNow, OrderStatus = (int)OrderStatus.Pending }
        };
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.UserId == userId).Should().BeTrue();
        result.First().PlacedAt.Should().BeAfter(result.Last().PlacedAt); // Verify ordering
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsOrdersWithStatus()
    {
        // Arrange
        var status = (int)OrderStatus.Pending;
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), OrderStatus = status, PlacedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), OrderStatus = status, PlacedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), OrderStatus = (int)OrderStatus.Completed, PlacedAt = DateTime.UtcNow }
        };
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(status);

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.OrderStatus == status).Should().BeTrue();
    }

    [Fact]
    public async Task GetByExternalUserRefAsync_ReturnsOrdersForRef()
    {
        // Arrange
        var externalRef = "stripe_cust_123";
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ExternalUserRef = externalRef, PlacedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ExternalUserRef = "other", PlacedAt = DateTime.UtcNow }
        };
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByExternalUserRefAsync(externalRef);

        // Assert
        result.Should().HaveCount(1);
        result.First().ExternalUserRef.Should().Be(externalRef);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_OrderExists_ReturnsOrderWithItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), Quantity = 2, UnitPrice = 15m }
            }
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithItemsAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.OrderItems.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_OrderNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdWithItemsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}
