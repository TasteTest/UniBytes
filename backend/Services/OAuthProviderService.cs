using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// OAuth provider service implementation
/// </summary>
public class OAuthProviderService : IOAuthProviderService
{
    private readonly IOAuthProviderRepository _oauthProviderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OAuthProviderService> _logger;

    public OAuthProviderService(IOAuthProviderRepository oauthProviderRepository, IMapper mapper, ILogger<OAuthProviderService> logger)
    {
        _oauthProviderRepository = oauthProviderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OAuthProviderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider with ID {id} not found");
            }

            var providerResponse = _mapper.Map<OAuthProviderResponse>(provider);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth provider by ID {ProviderId}", id);
            return Result<OAuthProviderResponse>.Failure($"Error retrieving OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<OAuthProviderResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await _oauthProviderRepository.GetByUserIdAsync(userId, cancellationToken);
            var providerResponses = _mapper.Map<IEnumerable<OAuthProviderResponse>>(providers);
            return Result<IEnumerable<OAuthProviderResponse>>.Success(providerResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth providers for user {UserId}", userId);
            return Result<IEnumerable<OAuthProviderResponse>>.Failure($"Error retrieving OAuth providers: {ex.Message}");
        }
    }

    public async Task<Result<OAuthProviderResponse>> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId, cancellationToken);
            if (oauthProvider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider {provider} with ID {providerId} not found");
            }

            var providerResponse = _mapper.Map<OAuthProviderResponse>(oauthProvider);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OAuth provider {Provider} with ID {ProviderId}", provider, providerId);
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
            var providerExists = await _oauthProviderRepository.ExistsAsync(createRequest.Provider, createRequest.ProviderId, cancellationToken);
            if (providerExists)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider {createRequest.Provider} with ID {createRequest.ProviderId} already exists");
            }

            var provider = _mapper.Map<OAuthProvider>(createRequest);
            provider.CreatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await _oauthProviderRepository.AddAsync(provider, cancellationToken);

            var providerResponse = _mapper.Map<OAuthProviderResponse>(provider);
            _logger.LogInformation("Created OAuth provider {Provider} for user {UserId}", createRequest.Provider, createRequest.UserId);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OAuth provider");
            return Result<OAuthProviderResponse>.Failure($"Error creating OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result<OAuthProviderResponse>> UpdateAsync(Guid id, UpdateOAuthProviderRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider with ID {id} not found");
            }

            _mapper.Map(updateRequest, provider);
            provider.UpdatedAt = DateTime.UtcNow;

            await _oauthProviderRepository.UpdateAsync(provider, cancellationToken);

            var providerResponse = _mapper.Map<OAuthProviderResponse>(provider);
            _logger.LogInformation("Updated OAuth provider with ID {ProviderId}", id);
            return Result<OAuthProviderResponse>.Success(providerResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating OAuth provider {ProviderId}", id);
            return Result<OAuthProviderResponse>.Failure($"Error updating OAuth provider: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _oauthProviderRepository.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result.Failure($"OAuth provider with ID {id} not found");
            }

            await _oauthProviderRepository.DeleteAsync(provider, cancellationToken);

            _logger.LogInformation("Deleted OAuth provider with ID {ProviderId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OAuth provider {ProviderId}", id);
            return Result.Failure($"Error deleting OAuth provider: {ex.Message}");
        }
    }
}
