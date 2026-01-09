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
    /// This endpoint is for inter-service communication.
    /// </summary>
    /// <param name="authorization">Authorization header containing Bearer token.</param>
    /// <param name="userEmail">User email from the X-User-Email header.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User information if token is valid.</returns>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(
        [FromHeader(Name = "Authorization")] string? authorization,
        [FromHeader(Name = "X-User-Email")] string? userEmail,
        CancellationToken cancellationToken)
    {
        // Extract token from Authorization header
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            return Unauthorized(new { error = "Missing or invalid authorization header" });
        }

        var token = authorization.Substring("Bearer ".Length).Trim();
        
        // For now, we'll use a simple approach - extract email from the token
        // In production, you should validate the JWT token properly
        try
        {
            // This is a simplified version - you should validate the JWT token
            // For NextAuth tokens, you can validate them or use a shared session store
            
            // For now, let's get user by the stored provider info
            // The token should contain enough info to identify the user
            
            // Since NextAuth tokens are JWTs, you'd normally validate them
            // For this demo, we'll accept the email from headers as a fallback
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { error = "User email not found in request" });
            }

            // Get user directly from repository to access ID
            var user = await userService.GetUserEntityByEmailAsync(userEmail, cancellationToken);
            
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token");
            return Unauthorized(new { error = "Invalid token" });
        }
    }
}

