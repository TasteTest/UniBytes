using AutoMapper;
using backend_user.Common;
using backend_user.Common.Enums;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;
using backend_user.Model;
using backend_user.Repositories.Interfaces;
using backend_user.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_user.Services;

/// <summary>
/// OAuth provider service implementation
/// </summary>
public class OAuthProviderService : IOAuthProviderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OAuthProviderService> _logger;

    public OAuthProviderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OAuthProviderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OAuthProviderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _unitOfWork.OAuthProviders.GetByIdAsync(id, cancellationToken);
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
            var providers = await _unitOfWork.OAuthProviders.GetByUserIdAsync(userId, cancellationToken);
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
            var oauthProvider = await _unitOfWork.OAuthProviders.GetByProviderAndProviderIdAsync(provider, providerId, cancellationToken);
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
            var userExists = await _unitOfWork.Users.AnyAsync(u => u.Id == createRequest.UserId, cancellationToken);
            if (!userExists)
            {
                return Result<OAuthProviderResponse>.Failure($"User with ID {createRequest.UserId} not found");
            }

            // Check if OAuth provider already exists
            var providerExists = await _unitOfWork.OAuthProviders.ExistsAsync(createRequest.Provider, createRequest.ProviderId, cancellationToken);
            if (providerExists)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider {createRequest.Provider} with ID {createRequest.ProviderId} already exists");
            }

            var provider = _mapper.Map<OAuthProvider>(createRequest);
            provider.CreatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OAuthProviders.AddAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var provider = await _unitOfWork.OAuthProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result<OAuthProviderResponse>.Failure($"OAuth provider with ID {id} not found");
            }

            _mapper.Map(updateRequest, provider);
            provider.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OAuthProviders.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var provider = await _unitOfWork.OAuthProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null)
            {
                return Result.Failure($"OAuth provider with ID {id} not found");
            }

            await _unitOfWork.OAuthProviders.DeleteAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
