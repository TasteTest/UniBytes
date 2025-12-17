namespace backend.Common.Enums;

/// <summary>
/// User roles for access control
/// </summary>
public enum UserRole
{
    User = 0,   // Default - can place orders
    Chef = 1,   // Kitchen staff - manages order status
    Admin = 2   // Full access - manages users, menu, roles
}
