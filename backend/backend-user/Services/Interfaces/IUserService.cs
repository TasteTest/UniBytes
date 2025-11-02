using backend_user.Common;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;
using backend_user.Model;

namespace backend_user.Services.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserEntityByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserResponse>>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserResponse>>> GetAdminUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest createRequest, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest updateRequest, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
}
