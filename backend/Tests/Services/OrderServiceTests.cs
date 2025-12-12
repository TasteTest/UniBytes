using backend.Common;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.Services.Interfaces;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;
using backend.DTOs.Loyalty.Request;
using backend.DTOs.Loyalty.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ILoyaltyAccountService> _mockLoyaltyAccountService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockLoyaltyAccountService = new Mock<ILoyaltyAccountService>();
        _mockLogger = new Mock<ILogger<OrderService>>();

        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockLoyaltyAccountService.Object,
            _mockLogger.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_OrderExists_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100.50m,
            Currency = "RON",
            PaymentStatus = 0,
            OrderStatus = 0,
            PlacedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(orderId);
        result.Data.TotalAmount.Should().Be(100.50m);
    }

    [Fact]
    public async Task GetByIdAsync_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task GetByIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get order");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TotalAmount = 50m, Currency = "RON", OrderItems = new List<OrderItem>() },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), TotalAmount = 75m, Currency = "RON", OrderItems = new List<OrderItem>() }
        };

        _mockOrderRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        _mockOrderRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get orders");
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalAmount = 100m, Currency = "RON", OrderItems = new List<OrderItem>() }
        };

        _mockOrderRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get user orders");
    }

    #endregion

    #region GetByStatusAsync Tests

    [Fact]
    public async Task GetByStatusAsync_ReturnsOrdersByStatus()
    {
        // Arrange
        var status = (int)OrderStatus.Pending;
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), OrderStatus = status, TotalAmount = 50m, Currency = "RON", OrderItems = new List<OrderItem>() }
        };

        _mockOrderRepository.Setup(x => x.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetByStatusAsync(status);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByStatusAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var status = (int)OrderStatus.Pending;
        _mockOrderRepository.Setup(x => x.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.GetByStatusAsync(status);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get orders by status");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createRequest = new CreateOrderRequest(
            userId,
            new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(Guid.NewGuid(), "Pizza", 25.00m, 2, null),
                new CreateOrderItemRequest(Guid.NewGuid(), "Drink", 5.00m, 3, null)
            },
            "RON",
            null
        );

        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        // Act
        var result = await _orderService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(65.00m); // 25*2 + 5*3
        result.Data.OrderItems.Should().HaveCount(2);

        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(
            Guid.NewGuid(),
            new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(Guid.NewGuid(), "Pizza", 25.00m, 1, null)
            },
            "RON",
            null
        );

        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create order");
    }

    #endregion

    #region UpdateStatusAsync Tests

    [Fact]
    public async Task UpdateStatusAsync_OrderExists_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Pending,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OrderStatus.Should().Be("Confirmed");

        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_AwardsLoyaltyPoints()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLoyaltyAccountService.Setup(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(new LoyaltyAccountResponse()));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.OrderStatus.Should().Be("Completed");

        _mockLoyaltyAccountService.Verify(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_CreatesLoyaltyAccountIfNotExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // First call fails because account not found
        _mockLoyaltyAccountService.SetupSequence(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Failure("Account not found"))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(new LoyaltyAccountResponse()));

        _mockLoyaltyAccountService.Setup(x => x.CreateAsync(It.IsAny<CreateLoyaltyAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(new LoyaltyAccountResponse()));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockLoyaltyAccountService.Verify(x => x.CreateAsync(It.IsAny<CreateLoyaltyAccountRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task UpdateStatusAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Confirmed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update order status");
    }

    [Fact]
    public async Task UpdateStatusAsync_AlreadyCompleted_DoesNotAwardPointsAgain()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Completed,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockLoyaltyAccountService.Verify(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region CancelAsync Tests

    [Fact]
    public async Task CancelAsync_PendingOrder_CancelsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Pending,
            OrderItems = new List<OrderItem>()
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CancelAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.OrderStatus.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAsync_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.CancelAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task CancelAsync_NotPendingOrder_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Confirmed,
            OrderItems = new List<OrderItem>()
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CancelAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Only pending orders can be cancelled");
    }

    [Fact]
    public async Task CancelAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.CancelAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to cancel order");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_OrderExists_DeletesOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "RON",
            OrderItems = new List<OrderItem>()
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.DeleteAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.DeleteAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockOrderRepository.Verify(x => x.DeleteAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.DeleteAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task DeleteAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.DeleteAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to delete order");
    }

    #endregion

    #region GetByExternalUserRefAsync Tests

    [Fact]
    public async Task GetByExternalUserRefAsync_ReturnsOrders()
    {
        // Arrange
        var externalUserRef = "external-123";
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ExternalUserRef = externalUserRef, TotalAmount = 50m, Currency = "RON", OrderItems = new List<OrderItem>() }
        };

        _mockOrderRepository.Setup(x => x.GetByExternalUserRefAsync(externalUserRef, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetByExternalUserRefAsync(externalUserRef);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByExternalUserRefAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var externalUserRef = "external-123";
        _mockOrderRepository.Setup(x => x.GetByExternalUserRefAsync(externalUserRef, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _orderService.GetByExternalUserRefAsync(externalUserRef);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get orders by external user reference");
    }

    #endregion

    #region AwardLoyaltyPoints Edge Cases

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_NoUserIdDoesNotAwardPoints()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.Empty,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockLoyaltyAccountService.Verify(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_ZeroAmountDoesNotAwardPoints()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 0.50m, // Less than 1, rounds to 0 points
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockLoyaltyAccountService.Verify(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_LoyaltyServiceThrowsException_StillSucceeds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLoyaltyAccountService.Setup(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Loyalty service error"));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Order update should still succeed
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_LoyaltyPointsFailure_LogsWarning()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLoyaltyAccountService.Setup(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Failure("Failed to add points"));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Order update should still succeed
    }

    [Fact]
    public async Task UpdateStatusAsync_ToCompleted_CreateAccountFailure_DoesNotRetryAddPoints()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 100m,
            Currency = "RON",
            OrderStatus = (int)OrderStatus.Preparing,
            OrderItems = new List<OrderItem>()
        };

        var updateRequest = new UpdateOrderStatusRequest((int)OrderStatus.Completed);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLoyaltyAccountService.Setup(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Failure("Account not found"));

        _mockLoyaltyAccountService.Setup(x => x.CreateAsync(It.IsAny<CreateLoyaltyAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Failure("Failed to create account"));

        // Act
        var result = await _orderService.UpdateStatusAsync(orderId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // AddPoints called once, CreateAsync called once, but retry not attempted since create failed
        _mockLoyaltyAccountService.Verify(x => x.AddPointsAsync(It.IsAny<AddPointsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region MapToResponse Tests

    [Fact]
    public async Task GetByIdAsync_WithOrderItems_MapsItemsCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = Guid.NewGuid(),
            Name = "Test Item",
            UnitPrice = 10.00m,
            Quantity = 2,
            TotalPrice = 20.00m
        };

        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 20.00m,
            Currency = "RON",
            PaymentStatus = (int)PaymentStatus.Processing,
            OrderStatus = (int)OrderStatus.Pending,
            OrderItems = new List<OrderItem> { orderItem }
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.OrderItems.Should().HaveCount(1);
        result.Data.OrderItems.First().Name.Should().Be("Test Item");
        result.Data.OrderItems.First().UnitPrice.Should().Be(10.00m);
        result.Data.PaymentStatus.Should().Be("Processing");
        result.Data.OrderStatus.Should().Be("Pending");
    }

    #endregion
}
