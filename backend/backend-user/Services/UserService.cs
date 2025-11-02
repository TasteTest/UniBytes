using AutoMapper;
using backend_user.Common;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;
using backend_user.Model;
using backend_user.Repositories.Interfaces;
using backend_user.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_user.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return Result<UserResponse>.Failure($"User with ID {id} not found");
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", id);
            return Result<UserResponse>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return Result<UserResponse>.Failure($"User with email {email} not found");
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return Result<UserResponse>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<User?> GetUserEntityByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user entity by email {Email}", email);
            return null;
        }
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            var userResponses = _mapper.Map<IEnumerable<UserResponse>>(users);
            return Result<IEnumerable<UserResponse>>.Success(userResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return Result<IEnumerable<UserResponse>>.Failure($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _unitOfWork.Users.GetActiveUsersAsync(cancellationToken);
            var userResponses = _mapper.Map<IEnumerable<UserResponse>>(users);
            return Result<IEnumerable<UserResponse>>.Success(userResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            return Result<IEnumerable<UserResponse>>.Failure($"Error retrieving active users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAdminUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _unitOfWork.Users.GetAdminUsersAsync(cancellationToken);
            var userResponses = _mapper.Map<IEnumerable<UserResponse>>(users);
            return Result<IEnumerable<UserResponse>>.Success(userResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin users");
            return Result<IEnumerable<UserResponse>>.Failure($"Error retrieving admin users: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if email already exists
            var emailExists = await _unitOfWork.Users.EmailExistsAsync(createRequest.Email, cancellationToken);
            if (emailExists)
            {
                return Result<UserResponse>.Failure($"User with email {createRequest.Email} already exists");
            }

            var user = _mapper.Map<User>(createRequest);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userResponse = _mapper.Map<UserResponse>(user);
            _logger.LogInformation("Created new user with email {Email}", user.Email);
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Result<UserResponse>.Failure($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return Result<UserResponse>.Failure($"User with ID {id} not found");
            }

            // Check if email is being updated and if it already exists
            if (!string.IsNullOrEmpty(updateRequest.Email) && updateRequest.Email != user.Email)
            {
                var emailExists = await _unitOfWork.Users.EmailExistsAsync(updateRequest.Email, cancellationToken);
                if (emailExists)
                {
                    return Result<UserResponse>.Failure($"User with email {updateRequest.Email} already exists");
                }
            }

            _mapper.Map(updateRequest, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userResponse = _mapper.Map<UserResponse>(user);
            _logger.LogInformation("Updated user with ID {UserId}", id);
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return Result<UserResponse>.Failure($"Error updating user: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return Result.Failure($"User with ID {id} not found");
            }

            await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted user with ID {UserId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Result.Failure($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<Result> UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.Users.UpdateLastLoginAsync(userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated last login for user {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
            return Result.Failure($"Error updating last login: {ex.Message}");
        }
    }
}
