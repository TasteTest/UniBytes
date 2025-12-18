using backend.Common;
using backend.Common.Enums;
using backend.Controllers;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly OrdersController _controller;

    public OrderControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockUserService = new Mock<IUserService>();
        _controller = new OrdersController(_mockOrderService.Object, _mockUserService.Object);
        
        // Setup HttpContext to prevent NullReferenceException when accessing Request.Headers
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region CreateOrder Tests

    [Fact]
    public async Task CreateOrder_NullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateOrder(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Order data is missing.");
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(Guid.NewGuid(), "Test Item", 10.00m, 2, null)
            },
            "USD",
            null
        );

        var response = new OrderResponse
        {
            Id = orderId,
            UserId = request.UserId,
            TotalAmount = 20.00m,
            Currency = "USD",
            PaymentStatus = "Processing",
            OrderStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _mockOrderService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(response));

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(_controller.GetOrder));
        createdResult.RouteValues!["id"].Should().Be(orderId);
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task CreateOrder_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(Guid.NewGuid(), "Test Item", 10.00m, 2, null)
            },
            "USD",
            null
        );

        _mockOrderService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Failure("Failed to create order"));

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Failed to create order");
    }

    #endregion

    #region GetOrder Tests

    [Fact]
    public async Task GetOrder_OrderExists_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var response = new OrderResponse
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Currency = "USD",
            PaymentStatus = "Succeeded",
            OrderStatus = "Completed",
            CreatedAt = DateTime.UtcNow
        };

        _mockOrderService.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(response));

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetOrder_OrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderService.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Failure($"Order with ID {orderId} not found"));

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult.Value!.ToString().Should().Contain(orderId.ToString());
    }

    #endregion

    #region GetAllOrders Tests

    [Fact]
    public async Task GetAllOrders_ReturnsOk()
    {
        // Arrange
        var orders = new List<OrderResponse>
        {
            new OrderResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TotalAmount = 25.00m,
                Currency = "USD",
                PaymentStatus = "Processing",
                OrderStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new OrderResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TotalAmount = 75.00m,
                Currency = "USD",
                PaymentStatus = "Succeeded",
                OrderStatus = "Completed",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockOrderService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Success(orders));

        // Act
        var result = await _controller.GetAllOrders();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetAllOrders_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        _mockOrderService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Failure("Error retrieving orders"));

        // Act
        var result = await _controller.GetAllOrders();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Error retrieving orders");
    }

    #endregion

    #region GetUserOrders Tests

    [Fact]
    public async Task GetUserOrders_UserHasOrders_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<OrderResponse>
        {
            new OrderResponse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TotalAmount = 30.00m,
                Currency = "USD",
                PaymentStatus = "Succeeded",
                OrderStatus = "Completed",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockOrderService.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Success(orders));

        // Act
        var result = await _controller.GetUserOrders(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetUserOrders_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockOrderService.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Failure("Error retrieving user orders"));

        // Act
        var result = await _controller.GetUserOrders(userId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Error retrieving user orders");
    }

    #endregion

    #region GetOrdersByStatus Tests

    [Fact]
    public async Task GetOrdersByStatus_ReturnsOk()
    {
        // Arrange
        var status = (int)OrderStatus.Pending;
        var orders = new List<OrderResponse>
        {
            new OrderResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TotalAmount = 45.00m,
                Currency = "USD",
                PaymentStatus = "Processing",
                OrderStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockOrderService.Setup(x => x.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Success(orders));

        // Act
        var result = await _controller.GetOrdersByStatus(status);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetOrdersByStatus_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var status = 99; // Invalid status
        _mockOrderService.Setup(x => x.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<OrderResponse>>.Failure("Invalid status"));

        // Act
        var result = await _controller.GetOrdersByStatus(status);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Invalid status");
    }

    #endregion

    #region UpdateOrderStatus Tests

    [Fact]
    public async Task UpdateOrderStatus_NullRequest_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Status data is missing.");
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidRequest_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest((int)OrderStatus.Confirmed);

        var response = new OrderResponse
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 60.00m,
            Currency = "USD",
            PaymentStatus = "Succeeded",
            OrderStatus = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _mockOrderService.Setup(x => x.UpdateStatusAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(response));

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task UpdateOrderStatus_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest((int)OrderStatus.Confirmed);

        _mockOrderService.Setup(x => x.UpdateStatusAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Failure($"Order with ID {orderId} not found"));

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
        badRequest.Value!.ToString().Should().Contain(orderId.ToString());
    }

    #endregion

    #region CancelOrder Tests

    [Fact]
    public async Task CancelOrder_Success_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var response = new OrderResponse
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            TotalAmount = 40.00m,
            Currency = "USD",
            PaymentStatus = "Cancelled",
            OrderStatus = "Cancelled",
            CreatedAt = DateTime.UtcNow
        };

        _mockOrderService.Setup(x => x.CancelAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(response));

        // Act
        var result = await _controller.CancelOrder(orderId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task CancelOrder_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderService.Setup(x => x.CancelAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Failure("Order cannot be cancelled"));

        // Act
        var result = await _controller.CancelOrder(orderId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Order cannot be cancelled");
    }

    #endregion

    #region DeleteOrder Tests

    [Fact]
    public async Task DeleteOrder_Success_ReturnsNoContent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderService.Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.DeleteOrder(orderId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteOrder_ServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderService.Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure($"Order with ID {orderId} not found"));

        // Act
        var result = await _controller.DeleteOrder(orderId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
        badRequest.Value!.ToString().Should().Contain(orderId.ToString());
    }

    #endregion
}
