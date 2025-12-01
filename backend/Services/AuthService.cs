using AutoMapper;
using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Data;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IOAuthProviderRepository _oauthProviderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IOAuthProviderRepository oauthProviderRepository,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _context = context;
        _userRepository = userRepository;
        _oauthProviderRepository = oauthProviderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use execution strategy to wrap the entire transaction operation
            // This is required when retry on failure is enabled
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                // Check if OAuth provider already exists
                var existingOAuthProvider = await _oauthProviderRepository
                    .GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId, cancellationToken);

                User user;
                bool isNewUser = false;

                if (existingOAuthProvider != null)
                {
                    // User already exists with this OAuth provider
                    var existingUser = await _userRepository.GetByIdAsync(existingOAuthProvider.UserId, cancellationToken);
                    if (existingUser == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<AuthResponse>.Failure("User not found");
                    }
                    user = existingUser;

                    // Update OAuth provider tokens
                    existingOAuthProvider.AccessToken = request.AccessToken;
                    existingOAuthProvider.RefreshToken = request.RefreshToken;
                    existingOAuthProvider.TokenExpiresAt = request.TokenExpiresAt;
                    existingOAuthProvider.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(existingOAuthProvider).State = EntityState.Modified;

                    // Update user last login
                    user.LastLoginAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    // Update user profile if not set
                    if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(request.FirstName))
                    {
                        user.FirstName = request.FirstName;
                    }
                    if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(request.LastName))
                    {
                        user.LastName = request.LastName;
                    }
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(request.AvatarUrl))
                    {
                        user.AvatarUrl = request.AvatarUrl;
                    }

                    _context.Entry(user).State = EntityState.Modified;

                    _logger.LogInformation("Existing user {Email} authenticated with Google", user.Email);
                }
                else
                {
                    // Check if user exists by email
                    var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

                    if (existingUserByEmail != null)
                    {
                        user = existingUserByEmail;
                        // User exists but doesn't have this OAuth provider linked
                        // Link the OAuth provider to existing user
                        var newOAuthProvider = new OAuthProvider
                        {
                            UserId = user.Id,
                            Provider = request.Provider,
                            ProviderId = request.ProviderId,
                            ProviderEmail = request.ProviderEmail,
                            AccessToken = request.AccessToken,
                            RefreshToken = request.RefreshToken,
                            TokenExpiresAt = request.TokenExpiresAt,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);

                        // Update user last login
                        user.LastLoginAt = DateTime.UtcNow;
                        user.UpdatedAt = DateTime.UtcNow;
                        _context.Entry(user).State = EntityState.Modified;

                        _logger.LogInformation("Linked Google OAuth to existing user {Email}", user.Email);
                    }
                    else
                    {
                        // Create new user
                        user = new User
                        {
                            Email = request.Email,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            AvatarUrl = request.AvatarUrl,
                            IsActive = true,
                            IsAdmin = false,
                            LastLoginAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _context.Users.AddAsync(user, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken); // Save to get user ID

                        // Create OAuth provider
                        var newOAuthProvider = new OAuthProvider
                        {
                            UserId = user.Id,
                            Provider = request.Provider,
                            ProviderId = request.ProviderId,
                            ProviderEmail = request.ProviderEmail,
                            AccessToken = request.AccessToken,
                            RefreshToken = request.RefreshToken,
                            TokenExpiresAt = request.TokenExpiresAt,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);

                        isNewUser = true;
                        _logger.LogInformation("Created new user {Email} with Google OAuth", user.Email);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var userResponse = _mapper.Map<UserResponse>(user);
                var authResponse = new AuthResponse
                {
                    UserId = user.Id.ToString(),
                    User = userResponse,
                    IsNewUser = isNewUser,
                    Message = isNewUser ? "User created successfully" : "User authenticated successfully"
                };

                return Result<AuthResponse>.Success(authResponse);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication for email {Email}", request.Email);
            return Result<AuthResponse>.Failure($"Authentication error: {ex.Message}");
        }
    }
}

