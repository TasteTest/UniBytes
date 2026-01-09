namespace backend.Middleware;

using backend.Common.Enums;

/// <summary>
/// Represents an authenticated user extracted from the JWT token.
/// Stored in HttpContext.Items for the duration of a single request.
/// </summary>
public class AuthenticatedUser
{
    /// <summary>
    /// The user's unique identifier from the database.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// The user's role (User=0, Chef=1, Admin=2).
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// The user's first name (optional).
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// The user's last name (optional).
    /// </summary>
    public string? LastName { get; set; }
}
