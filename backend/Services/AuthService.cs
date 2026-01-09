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
            // User already exists with this OAuth provider
            var existingUser = await userRepository.GetByIdAsync(existingOAuthProvider.UserId, cancellationToken);
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

            context.Entry(existingOAuthProvider).State = EntityState.Modified;

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

            context.Entry(user).State = EntityState.Modified;

            logger.LogInformation("Existing user {Email} authenticated with Google", user.Email);
        }
        else
        {
            // Check if user exists by email
            var existingUserByEmail = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

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

                await context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);

                // Update user last login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                context.Entry(user).State = EntityState.Modified;

                logger.LogInformation("Linked Google OAuth to existing user {Email}", user.Email);
            }
            else
            {
                // Create new user
                // First admin is hardcoded
                var isFirstAdmin = string.Equals(request.Email, "lisaioanamercas@gmail.com", StringComparison.OrdinalIgnoreCase);
                
                user = new User
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
                await context.SaveChangesAsync(cancellationToken); // Save to get user ID

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

                await context.OAuthProviders.AddAsync(newOAuthProvider, cancellationToken);

                isNewUser = true;
                logger.LogInformation("Created new user {Email} with Google OAuth", user.Email);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var userResponse = mapper.Map<UserResponse>(user);
        var authResponse = new AuthResponse
        {
            UserId = user.Id.ToString(),
            User = userResponse,
            IsNewUser = isNewUser,
            Message = isNewUser ? "User created successfully" : "User authenticated successfully"
        };

        return Result<AuthResponse>.Success(authResponse);
    }
}

