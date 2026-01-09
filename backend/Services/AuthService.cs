using AutoMapper;
using backend.Common;
using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.Auth.Request;
using backend.DTOs.Auth.Response;
using backend.DTOs.User.Response;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService(
    ApplicationDbContext context,
    IUserRepository userRepository,
    IOAuthProviderRepository oauthProviderRepository,
    IMapper mapper,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use execution strategy to wrap the entire transaction operation
            // This is required when retry on failure is enabled
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () => await ProcessGoogleAuthentication(request, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Google authentication for email {Email}", request.Email);
            return Result<AuthResponse>.Failure($"Authentication error: {ex.Message}");
        }
    }

    private async Task<Result<AuthResponse>> ProcessGoogleAuthentication(GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Check if OAuth provider already exists
        var existingOAuthProvider = await oauthProviderRepository
            .GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId, cancellationToken);

        User user;
        bool isNewUser = false;

        if (existingOAuthProvider != null)
        {
            var existingUser = await HandleExistingOAuthProviderAsync(existingOAuthProvider, request, cancellationToken);
            if (existingUser == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<AuthResponse>.Failure("User not found");
            }
            user = existingUser;
        }
        else
        {
            var existingUserByEmail = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (existingUserByEmail != null)
            {
                user = await HandleExistingUserLinkingAsync(existingUserByEmail, request, cancellationToken);
            }
            else
            {
                user = await CreateNewUserAndProviderAsync(request, cancellationToken);
                isNewUser = true;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var authResponse = GenerateAuthResponse(user, isNewUser);
        return Result<AuthResponse>.Success(authResponse);
    }

    private async Task<User?> HandleExistingOAuthProviderAsync(OAuthProvider existingOAuthProvider, GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByIdAsync(existingOAuthProvider.UserId, cancellationToken);
        if (existingUser == null) return null;

        existingOAuthProvider.AccessToken = request.AccessToken;
        existingOAuthProvider.RefreshToken = request.RefreshToken;
        existingOAuthProvider.TokenExpiresAt = request.TokenExpiresAt;
        existingOAuthProvider.UpdatedAt = DateTime.UtcNow;

        context.Entry(existingOAuthProvider).State = EntityState.Modified;

        existingUser.LastLoginAt = DateTime.UtcNow;
        existingUser.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(request.FirstName)) existingUser.FirstName = request.FirstName;
        if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(request.LastName)) existingUser.LastName = request.LastName;
        if (string.IsNullOrEmpty(existingUser.AvatarUrl) && !string.IsNullOrEmpty(request.AvatarUrl)) existingUser.AvatarUrl = request.AvatarUrl;

        context.Entry(existingUser).State = EntityState.Modified;
        logger.LogInformation("Existing user {Email} authenticated with Google", existingUser.Email);
        
        return existingUser;
    }

    private async Task<User> HandleExistingUserLinkingAsync(User user, GoogleAuthRequest request, CancellationToken cancellationToken)
    {
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

        await context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        context.Entry(user).State = EntityState.Modified;

        logger.LogInformation("Linked Google OAuth to existing user {Email}", user.Email);
        return user;
    }

    private async Task<User> CreateNewUserAndProviderAsync(GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        var isFirstAdmin = string.Equals(request.Email, "lisaioanamercas@gmail.com", StringComparison.OrdinalIgnoreCase);

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AvatarUrl = request.AvatarUrl,
            IsActive = true,
            Role = isFirstAdmin ? Common.Enums.UserRole.Admin : Common.Enums.UserRole.User,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

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

        await context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);
        logger.LogInformation("Created new user {Email} with Google authentication", user.Email);
        return user;
    }

    private AuthResponse GenerateAuthResponse(User user, bool isNewUser)
    {
        var userResponse = mapper.Map<UserResponse>(user);
        return new AuthResponse
        {
            UserId = user.Id.ToString(),
            User = userResponse,
            IsNewUser = isNewUser,
            Message = isNewUser ? "User created successfully" : "User authenticated successfully"
        };
    }
}

