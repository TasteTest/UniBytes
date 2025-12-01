using AutoMapper;
using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
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
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
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
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
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
            var users = await _userRepository.GetAllAsync(cancellationToken);
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
            var users = await _userRepository.GetActiveUsersAsync(cancellationToken);
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
            var users = await _userRepository.GetAdminUsersAsync(cancellationToken);
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
            var emailExists = await _userRepository.EmailExistsAsync(createRequest.Email, cancellationToken);
            if (emailExists)
            {
                return Result<UserResponse>.Failure($"User with email {createRequest.Email} already exists");
            }

            var user = _mapper.Map<User>(createRequest);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.AddAsync(user, cancellationToken);

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
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return Result<UserResponse>.Failure($"User with ID {id} not found");
            }

            // Check if email is being updated and if it already exists
            if (!string.IsNullOrEmpty(updateRequest.Email) && updateRequest.Email != user.Email)
            {
                var emailExists = await _userRepository.EmailExistsAsync(updateRequest.Email, cancellationToken);
                if (emailExists)
                {
                    return Result<UserResponse>.Failure($"User with email {updateRequest.Email} already exists");
                }
            }

            _mapper.Map(updateRequest, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

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
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return Result.Failure($"User with ID {id} not found");
            }

            await _userRepository.DeleteAsync(user, cancellationToken);

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
            await _userRepository.UpdateLastLoginAsync(userId, cancellationToken);

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
