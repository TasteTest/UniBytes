using backend.Services.Interfaces;
using backend.DTOs.Auth.Request;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IUserService userService,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    /// Authenticates or registers a user via Google OAuth.
    /// </summary>
    /// <param name="request">Google authentication request data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An authentication response.</returns>
    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.AuthenticateWithGoogleAsync(request, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Validates an access token and returns user information.
    /// This endpoint uses the JWT middleware for authentication.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User information if token is valid.</returns>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        // The JWT middleware has already validated the token and user
        // Get authenticated user from HttpContext (set by middleware)
        var authenticatedUser = HttpContext.Items["AuthenticatedUser"] as Middleware.AuthenticatedUser;
        
        if (authenticatedUser == null)
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        // Get full user details from database
        var user = await userService.GetUserEntityByEmailAsync(authenticatedUser.Email, cancellationToken);
        
        if (user == null)
        {
            return Unauthorized(new { error = "User not found" });
        }

        // Return user info with ID and role
        return Ok(new
        {
            id = user.Id.ToString(),
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            avatarUrl = user.AvatarUrl,
            role = (int)user.Role
        });
    }
}

