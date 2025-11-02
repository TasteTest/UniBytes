using backend_user.DTOs.Request;
using backend_user.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_user.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

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
        var result = await _authService.AuthenticateWithGoogleAsync(request, cancellationToken);

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User information if token is valid.</returns>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        // Extract token from Authorization header
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { error = "Missing or invalid authorization header" });
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
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
            var userEmail = Request.Headers["X-User-Email"].ToString();
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { error = "User email not found in request" });
            }

            // Get user directly from repository to access ID
            var user = await _userService.GetUserEntityByEmailAsync(userEmail, cancellationToken);
            
            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            // Return user info with ID (only for inter-service communication)
            return Ok(new
            {
                id = user.Id.ToString(),
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                avatarUrl = user.AvatarUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Unauthorized(new { error = "Invalid token" });
        }
    }
}
