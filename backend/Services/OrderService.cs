using backend.Common;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;
using backend.DTOs.Loyalty.Request;

namespace backend.Services;

/// <summary>
/// Order service implementation
/// </summary>
public class OrderService(
    IOrderRepository orderRepository,
    ILoyaltyAccountService loyaltyAccountService,
    ILogger<OrderService> logger)
    : IOrderService
{
    public async Task<Result<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return Result<OrderResponse>.Failure("Order not found");

            var response = MapToResponse(order);
            return Result<OrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting order by id");
            return Result<OrderResponse>.Failure("Failed to get order");
        }
    }

    public async Task<Result<IEnumerable<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderRepository.GetAllAsync(cancellationToken);
            var responses = orders.Select(MapToResponse);
            return Result<IEnumerable<OrderResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all orders");
            return Result<IEnumerable<OrderResponse>>.Failure("Failed to get orders");
        }
    }

    public async Task<Result<IEnumerable<OrderResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderRepository.GetByUserIdAsync(userId, cancellationToken);
            var responses = orders.Select(MapToResponse);
            return Result<IEnumerable<OrderResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting orders by user id");
            return Result<IEnumerable<OrderResponse>>.Failure("Failed to get user orders");
        }
    }

    public async Task<Result<IEnumerable<OrderResponse>>> GetByStatusAsync(int orderStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderRepository.GetByStatusAsync(orderStatus, cancellationToken);
            var responses = orders.Select(MapToResponse);
            return Result<IEnumerable<OrderResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting orders by status");
            return Result<IEnumerable<OrderResponse>>.Failure("Failed to get orders by status");
        }
    }

    public async Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = createRequest.UserId,
                Currency = createRequest.Currency?.ToLowerInvariant(),
                PaymentStatus = 0,
                OrderStatus = 0,
                PlacedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Metadata = createRequest.Metadata
            };
            
            decimal totalAmount = 0;
            foreach (var itemRequest in createRequest.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = itemRequest.MenuItemId,
                    Name = itemRequest.Name,
                    UnitPrice = itemRequest.UnitPrice,
                    Quantity = itemRequest.Quantity,
                    TotalPrice = itemRequest.UnitPrice * itemRequest.Quantity,
                    Modifiers = itemRequest.Modifiers,
                    IsReward = itemRequest.IsReward,
                    RewardId = itemRequest.RewardId,
                    CreatedAt = DateTime.UtcNow
                };
                
                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;
            }

            order.TotalAmount = totalAmount;

            var created = await orderRepository.AddAsync(order, cancellationToken);
            var response = MapToResponse(created);
            return Result<OrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order");
            return Result<OrderResponse>.Failure("Failed to create order");
        }
    }

    public async Task<Result<OrderResponse>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return Result<OrderResponse>.Failure("Order not found");

            var oldStatus = order.OrderStatus;
            order.OrderStatus = updateRequest.OrderStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await orderRepository.UpdateAsync(order, cancellationToken);
            
            // Award loyalty points when order is completed
            if (updateRequest.OrderStatus == (int)OrderStatus.Completed && oldStatus != (int)OrderStatus.Completed)
            {
                await AwardLoyaltyPointsAsync(order, cancellationToken);
            }
            
            var response = MapToResponse(order);
            return Result<OrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating order status");
            return Result<OrderResponse>.Failure("Failed to update order status");
        }
    }

    public async Task<Result<OrderResponse>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return Result<OrderResponse>.Failure("Order not found");
            if (order.OrderStatus != (int)OrderStatus.Pending)
                return Result<OrderResponse>.Failure("Only pending orders can be cancelled");
            else if (order.OrderStatus == 5)
                return Result<OrderResponse>.Failure("Order is already cancelled");

            order.CancelRequestedAt = DateTime.UtcNow;
            order.OrderStatus = 5;
            order.UpdatedAt = DateTime.UtcNow;

            await orderRepository.UpdateAsync(order, cancellationToken);
            
            var response = MapToResponse(order);
            return Result<OrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling order");
            return Result<OrderResponse>.Failure("Failed to cancel order");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return Result.Failure("Order not found");

            await orderRepository.DeleteAsync(order, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting order");
            return Result.Failure("Failed to delete order");
        }
    }

    public async Task<Result<IEnumerable<OrderResponse>>> GetByExternalUserRefAsync(string externalUserRef, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await orderRepository.GetByExternalUserRefAsync(externalUserRef, cancellationToken);
            var responses = orders.Select(MapToResponse);
            return Result<IEnumerable<OrderResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting orders by external user ref");
            return Result<IEnumerable<OrderResponse>>.Failure("Failed to get orders by external user reference");
        }
    }

    private async Task AwardLoyaltyPointsAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            if (order.UserId == Guid.Empty)
            {
                logger.LogWarning("Cannot award loyalty points for order {OrderId} - no user ID", order.Id);
                return;
            }

            // Calculate points: 1 point per dollar spent (rounded down)
            var points = (long)Math.Floor(order.TotalAmount);
            
            if (points <= 0)
            {
                logger.LogInformation("No loyalty points to award for order {OrderId} - amount too low", order.Id);
                return;
            }

            // Try to add points - if account doesn't exist, create it first
            var addPointsRequest = new AddPointsRequest
            {
                UserId = order.UserId,
                Points = points,
                Reason = $"Order completion: {order.Id}",
                ReferenceId = order.Id
            };

            var result = await loyaltyAccountService.AddPointsAsync(addPointsRequest, cancellationToken);
            
            // If failed because account doesn't exist, create it and retry
            if (!result.IsSuccess && result.Error?.Contains("not found") == true)
            {
                logger.LogInformation("Creating loyalty account for user {UserId}", order.UserId);
                var createRequest = new CreateLoyaltyAccountRequest
                {
                    UserId = order.UserId
                };
                
                var createResult = await loyaltyAccountService.CreateAsync(createRequest, cancellationToken);
                if (createResult.IsSuccess)
                {
                    // Retry adding points
                    result = await loyaltyAccountService.AddPointsAsync(addPointsRequest, cancellationToken);
                }
            }
            
            if (result.IsSuccess)
            {
                logger.LogInformation("Awarded {Points} loyalty points to user {UserId} for order {OrderId}", 
                    points, order.UserId, order.Id);
            }
            else
            {
                logger.LogWarning("Failed to award loyalty points for order {OrderId}: {Error}", 
                    order.Id, result.Error);
            }
        }
        catch (Exception ex)
        {
            // Don't fail the order status update if loyalty points fail
            logger.LogError(ex, "Error awarding loyalty points for order {OrderId}", order.Id);
        }
    }

    private static OrderResponse MapToResponse(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        ExternalUserRef = order.ExternalUserRef,
        TotalAmount = order.TotalAmount,
        Currency = order.Currency,
        PaymentStatus = ((PaymentStatus)order.PaymentStatus).ToString(),
        OrderStatus = ((OrderStatus)order.OrderStatus).ToString(),
        CreatedAt = order.CreatedAt,
        Metadata = order.Metadata,
        OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
        {
            Name = oi.Name,
            UnitPrice = oi.UnitPrice,
            Quantity = oi.Quantity,
            TotalPrice = oi.TotalPrice,
            Modifiers = oi.Modifiers,
            IsReward = oi.IsReward,
            RewardId = oi.RewardId
        }).ToList()
    };
}

