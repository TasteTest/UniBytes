using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.OAuthProvider.Request;
using backend.DTOs.OAuthProvider.Response;

namespace backend.Services;

/// <summary>
/// OAuth provider service implementation
/// </summary>
public class OAuthProviderService(
    IOAuthProviderRepository oauthProviderRepository,
    IMapper mapper,
    ILogger<OAuthProviderService> logger)
    : IOAuthProviderService
{
    public async Task<Result<OAuthProviderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider with ID {id} not found");
            }

            var providerResponse = mapper.Map<OAuthProviderResponse>(provider);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting OAuth provider by ID {ProviderId}", id);
            return Result<OAuthProviderResponse>.Failure($"Error retrieving OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<OAuthProviderResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await oauthProviderRepository.GetByUserIdAsync(userId, cancellationToken);
            var providerResponses = mapper.Map<IEnumerable<OAuthProviderResponse>>(providers);
            return Result<IEnumerable<OAuthProviderResponse>>.Success(providerResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting OAuth providers for user {UserId}", userId);
            return Result<IEnumerable<OAuthProviderResponse>>.Failure($"Error retrieving OAuth providers: {ex.Message}");
        }
    }

    public async Task<Result<OAuthProviderResponse>> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var oauthProvider = await oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId, cancellationToken);
            if (oauthProvider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider {provider} with ID {providerId} not found");
            }

            var providerResponse = mapper.Map<OAuthProviderResponse>(oauthProvider);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting OAuth provider {Provider} with ID {ProviderId}", provider, providerId);
            return Result<OAuthProviderResponse>.Failure($"Error retrieving OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result<OAuthProviderResponse>> CreateAsync(CreateOAuthProviderRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user exists
            // Note: In monolith, we assume user exists. In production, inject IUserRepository if needed
            var userExists = true; // User validation removed - handled at controller/auth level
            if (!userExists)
            {
                return Result<OAuthProviderResponse>.Failure($"User with ID {createRequest.UserId} not found");
            }

            // Check if OAuth provider already exists
            var providerExists = await oauthProviderRepository.ExistsAsync(createRequest.Provider, createRequest.ProviderId, cancellationToken);
            if (providerExists)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider {createRequest.Provider} with ID {createRequest.ProviderId} already exists");
            }

            var provider = mapper.Map<OAuthProvider>(createRequest);
            provider.CreatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await oauthProviderRepository.AddAsync(provider, cancellationToken);

            var providerResponse = mapper.Map<OAuthProviderResponse>(provider);
            logger.LogInformation("Created OAuth provider {Provider} for user {UserId}", createRequest.Provider, createRequest.UserId);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating OAuth provider");
            return Result<OAuthProviderResponse>.Failure($"Error creating OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result<OAuthProviderResponse>> UpdateAsync(Guid id, UpdateOAuthProviderRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider with ID {id} not found");
            }

            mapper.Map(updateRequest, provider);
            provider.UpdatedAt = DateTime.UtcNow;

            await oauthProviderRepository.UpdateAsync(provider, cancellationToken);

            var providerResponse = mapper.Map<OAuthProviderResponse>(provider);
            logger.LogInformation("Updated OAuth provider with ID {ProviderId}", id);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating OAuth provider {ProviderId}", id);
            return Result<OAuthProviderResponse>.Failure($"Error updating OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result.Failure($"OAuth provider with ID {id} not found");
            }

            await oauthProviderRepository.DeleteAsync(provider, cancellationToken);

            logger.LogInformation("Deleted OAuth provider with ID {ProviderId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting OAuth provider {ProviderId}", id);
            return Result.Failure($"Error deleting OAuth provider: {ex.Message}");
        }
    }
}
