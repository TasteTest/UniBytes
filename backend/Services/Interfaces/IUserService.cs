using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;

namespace backend.Services.Interfaces;

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
