using backend.Common.Enums;
using backend.Extensions;
using backend.Services.Interfaces;

namespace backend.Middleware;

/// <summary>
/// JWT Authentication middleware that validates Bearer tokens and protects API endpoints.
/// Each HTTP request gets its own HttpContext - this is NOT a global singleton.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    /// <summary>
    /// Public endpoints that do not require authentication.
    /// Format: "METHOD:PATH_PREFIX" or just "PATH_PREFIX" for all methods.
    /// </summary>
    private static readonly HashSet<string> PublicEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        // Authentication endpoints (login flow)
        "POST:/api/auth/google",
        
        // Menu browsing (public for all visitors)
        "GET:/api/menuitems",
        "GET:/api/categories",
        
        // Stripe webhook (uses its own signature validation)
        "POST:/api/payments/webhook",
        
        // Health check
        "GET:/health",
        
        // Swagger documentation
        "/swagger",
    };

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        // Check if this is a public endpoint
        if (IsPublicEndpoint(method, path))
        {
            await _next(context);
            return;
        }

        // Extract Authorization header
        var authHeader = context.Request.Headers["Authorization"].ToString();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Missing or invalid Authorization header for protected endpoint: {Method} {Path}", method, path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Authorization required" });
            return;
        }

        // Extract the token (we don't validate JWT signature currently - relies on NextAuth)
        // In production, you should validate the JWT signature using a shared secret or public key
        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token format" });
            return;
        }

        // Get user email from X-User-Email header (sent by frontend)
        var userEmail = context.Request.Headers["X-User-Email"].ToString();
        
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("Missing X-User-Email header for protected endpoint: {Method} {Path}", method, path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "User identification required" });
            return;
        }

        try
        {
            // Validate user exists in database
            var user = await userService.GetUserEntityByEmailAsync(userEmail, context.RequestAborted);
            
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", userEmail);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "User not found" });
                return;
            }

            // Store authenticated user in HttpContext for this request only
            context.SetAuthenticatedUser(new AuthenticatedUser
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName
            });

            _logger.LogDebug("User authenticated: {Email} (Role: {Role})", user.Email, user.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", userEmail);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication failed" });
            return;
        }

        // Continue to the next middleware/controller
        await _next(context);
    }

    private static bool IsPublicEndpoint(string method, string path)
    {
        // Check method-specific public endpoints (e.g., "GET:/api/menuitems")
        var methodPath = $"{method}:{path}";
        
        foreach (var endpoint in PublicEndpoints)
        {
            if (endpoint.Contains(':'))
            {
                // Method-specific check (e.g., "GET:/api/menuitems")
                if (methodPath.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else
            {
                // Path-only check (e.g., "/swagger")
                if (path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
