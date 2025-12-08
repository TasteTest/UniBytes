using Microsoft.AspNetCore.Mvc; 
using backend.Services.Interfaces;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct = default)
    {
        if (request == null) 
            return BadRequest("Order data is missing.");
        
        var result = await orderService.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct = default)
    {
        var result = await orderService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(CancellationToken ct = default)
    {
        var result = await orderService.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Data);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(Guid userId, CancellationToken ct = default)
    {
        var result = await orderService.GetByUserIdAsync(userId, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Data);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(int status, CancellationToken ct = default)
    {
        var result = await orderService.GetByStatusAsync(status, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Data);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        if (request == null)
            return BadRequest("Status data is missing.");
        
        var result = await orderService.UpdateStatusAsync(id, request, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Data);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct = default)
    {
        var result = await orderService.CancelAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken ct = default)
    {
        var result = await orderService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}
