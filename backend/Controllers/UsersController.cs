using backend.Services.Interfaces;
using backend.DTOs.User.Request;
using backend.DTOs.User.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Users controller for CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    private readonly ILogger<UsersController> _logger = logger;

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await userService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get active users only
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await userService.GetActiveUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get admin users only
    /// </summary>
    [HttpGet("admins")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdmins(CancellationToken cancellationToken)
    {
        var result = await userService.GetAdminUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var result = await userService.GetByEmailAsync(email, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest createRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await userService.CreateAsync(createRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, result.Data);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest updateRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await userService.UpdateAsync(id, updateRequest, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    [HttpPost("{id:guid}/last-login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLastLogin(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateLastLoginAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>
    /// Change a user's role (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUserRole(Guid id, [FromBody] SetRoleRequest request, CancellationToken cancellationToken)
    {
        // Only Admin can change roles
        var adminEmail = Request.Headers["X-User-Email"].ToString();
        if (string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized(new { error = "User email not found in request" });
        }

        var adminUser = await userService.GetUserEntityByEmailAsync(adminEmail, cancellationToken);
        if (adminUser == null || adminUser.Role != Common.Enums.UserRole.Admin)
        {
            return StatusCode(403, new { error = "Only Admin can change user roles" });
        }

        // Parse role from request
        if (!Enum.TryParse<Common.Enums.UserRole>(request.Role, true, out var newRole))
        {
            return BadRequest(new { error = "Invalid role. Valid roles: User, Chef, Admin" });
        }

        var result = await userService.SetUserRoleAsync(id, newRole, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }
}

/// <summary>
/// Request DTO for setting user role
/// </summary>
public class SetRoleRequest
{
    public string Role { get; set; } = "User";
}

