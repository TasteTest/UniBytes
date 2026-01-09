using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class OrderItemRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderItemRepository _repository;

    public OrderItemRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new OrderItemRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ReturnsItemsForOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var otherOrderId = Guid.NewGuid();

        var items = new List<OrderItem>
        {
            new OrderItem { Id = Guid.NewGuid(), OrderId = orderId, MenuItemId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m },
            new OrderItem { Id = Guid.NewGuid(), OrderId = orderId, MenuItemId = Guid.NewGuid(), Quantity = 2, UnitPrice = 20m },
            new OrderItem { Id = Guid.NewGuid(), OrderId = otherOrderId, MenuItemId = Guid.NewGuid(), Quantity = 1, UnitPrice = 5m }
        };

        await _context.Set<OrderItem>().AddRangeAsync(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().HaveCount(2);
        result.All(i => i.OrderId == orderId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByOrderIdAsync_NoItemsForOrder_ReturnsEmpty()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByMenuItemIdAsync_ReturnsItemsForMenuItem()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var otherMenuItemId = Guid.NewGuid();

        var items = new List<OrderItem>
        {
            new OrderItem { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), MenuItemId = menuItemId, Quantity = 1, UnitPrice = 10m },
            new OrderItem { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), MenuItemId = menuItemId, Quantity = 2, UnitPrice = 20m },
            new OrderItem { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), MenuItemId = otherMenuItemId, Quantity = 1, UnitPrice = 5m }
        };

        await _context.Set<OrderItem>().AddRangeAsync(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMenuItemIdAsync(menuItemId);

        // Assert
        result.Should().HaveCount(2);
        result.All(i => i.MenuItemId == menuItemId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByMenuItemIdAsync_NoItemsForMenuItem_ReturnsEmpty()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByMenuItemIdAsync(menuItemId);

        // Assert
        result.Should().BeEmpty();
    }
}
