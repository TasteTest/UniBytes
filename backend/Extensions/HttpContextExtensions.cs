using backend.Common.Enums;
using backend.Middleware;

namespace backend.Extensions;

/// <summary>
/// Extension methods for HttpContext to easily access authenticated user information.
/// </summary>
public static class HttpContextExtensions
{
    private const string AuthenticatedUserKey = "AuthenticatedUser";

    /// <summary>
    /// Sets the authenticated user for the current request.
    /// </summary>
    public static void SetAuthenticatedUser(this HttpContext context, AuthenticatedUser user)
    {
        context.Items[AuthenticatedUserKey] = user;
    }

    /// <summary>
    /// Gets the authenticated user for the current request.
    /// Returns null if no user is authenticated.
    /// </summary>
    public static AuthenticatedUser? GetAuthenticatedUser(this HttpContext context)
    {
        return context.Items.TryGetValue(AuthenticatedUserKey, out var user) 
            ? user as AuthenticatedUser 
            : null;
    }

    /// <summary>
    /// Checks if the current request has an authenticated user.
    /// </summary>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.GetAuthenticatedUser() != null;
    }

    /// <summary>
    /// Checks if the authenticated user has one of the specified roles.
    /// Returns false if user is not authenticated.
    /// </summary>
    public static bool IsInRole(this HttpContext context, params UserRole[] roles)
    {
        var user = context.GetAuthenticatedUser();
        if (user == null)
            return false;
        
        return roles.Contains(user.Role);
    }

    /// <summary>
    /// Gets the authenticated user's ID, or null if not authenticated.
    /// </summary>
    public static Guid? GetUserId(this HttpContext context)
    {
        return context.GetAuthenticatedUser()?.Id;
    }

    /// <summary>
    /// Gets the authenticated user's email, or null if not authenticated.
    /// </summary>
    public static string? GetUserEmail(this HttpContext context)
    {
        return context.GetAuthenticatedUser()?.Email;
    }

    /// <summary>
    /// Gets the authenticated user's role, or null if not authenticated.
    /// </summary>
    public static UserRole? GetUserRole(this HttpContext context)
    {
        return context.GetAuthenticatedUser()?.Role;
    }
}
